using MetadataExtractor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Sukul.Media.Backup.Shared
{

    public class Coordinator<S, D> where S : IMediaDiscovery where D : IMediaDestination
    {
        private Coordinator()
        { }

        private readonly S _source;
        private readonly D _destination;
        private readonly MultiThreadedFileWriter StatusFileWriter;
        private readonly ICollection<Task> _tasks;
        private readonly MultiThreadedFileWriter Logger;

        private enum FileProcessingStatus
        {
            Success, Fail
        }

        private Collection<MediaCreatedDateTimeMapping> CreatedDateTimeMappings = new Collection<MediaCreatedDateTimeMapping>()
        {
            new MediaCreatedDateTimeMapping { DirectoryName="QuickTime Movie Header", Name="Created", DateTimeParseFormat = "ddd MMM dd HH:mm:ss yyyy" },
            new MediaCreatedDateTimeMapping { DirectoryName="Exif IFD0", Name="Date/Time", DateTimeParseFormat = "yyyy:MM:dd HH:mm:ss" }
        };

        public Coordinator(S source, D destination, MultiThreadedFileWriter logger)
        {
            _source = source;
            _destination = destination;
            _tasks = new Collection<Task>();
            Logger = logger;
            StatusFileWriter = new MultiThreadedFileWriter();            
        }

        public async void ProcessAsync(string sourcePath, string destinationPath, bool recursive, bool processImages, bool processVideos, bool deleteAfterCopy, bool whatIf,
            CancellationToken cancellation)
        {
            try
            {
                var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                StatusFileWriter.Start($"{assemblyFolder}\\{DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")}-Processing-Result.csv", cancellation);
                StatusFileWriter.WriteLineWithoutTimeStamp($"File,Result,More Information");

                var options = new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 8,
                };
                var search = new ActionBlock<SourceMedia>((media) => CopyItemToDestination(media, destinationPath, _source, _destination, deleteAfterCopy, whatIf, cancellation), options);

                await foreach (var media in _source.AcquireAsync(sourcePath, true, processImages, processVideos))
                {
                    search.Post(media);
                }

                search.Complete();
                await search.Completion;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Logger.WriteLine(ex.ToString());    
                throw;
            }
        }

        public async Task CopyItemToDestination(SourceMedia media, string destinationPath, S source, D destination, bool deleteAfterCopy, bool whatIf, CancellationToken cancellation)
        {
            DateTime dateTime = default(DateTime);

            IEnumerable<MetadataExtractor.Tag> tags = default;
            using (var stream = new MemoryStream(media.Data))            
            {
                try
                {
                    tags = ImageMetadataReader.ReadMetadata(stream).SelectMany(d => d.Tags);
                }
                catch (Exception ex)
                {
                    StatusFileWriter.WriteLineWithoutTimeStamp($"\"{media.Reference}\",\"{FileProcessingStatus.Fail}\",\"{ex.ToString()}\"");
                }
            }

            string desinationFolder;

            var mappings = from m in CreatedDateTimeMappings
                           join t in tags
                           on new { m.DirectoryName, m.Name } equals new { t.DirectoryName, t.Name }
                           select (Mapping: m, Tag: t);

            if (!mappings.Any())
            {
                Trace.WriteLine($"Unable to determine creation date for {media.Reference} Skipping .. ");
                Logger.WriteLine($"Unable to determine creation date for {media.Reference} Skipping .. ");

                StatusFileWriter.WriteLineWithoutTimeStamp($"\"{media.Reference}\",\"{FileProcessingStatus.Fail}\",\"Unable to determine creation date for {media.Reference}\"");
            }
            else
            {
                var mapping = mappings.First();
                if (DateTime.TryParseExact(Convert.ToString(mapping.Tag.Description), mapping.Mapping.DateTimeParseFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
                {
                    desinationFolder = $"{destinationPath}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";

                    cancellation.ThrowIfCancellationRequested();

                    Trace.WriteLine($"Copying {media.Reference} to  {desinationFolder}");
                    Logger.WriteLine($"Copying {media.Reference} to {desinationFolder}");

                    if (!whatIf)
                    {
                        await destination.SaveAsync(desinationFolder, media.Data, media.Extension);
                    }
                    StatusFileWriter.WriteLineWithoutTimeStamp($"\"{media.Reference}\",\"{FileProcessingStatus.Success}\",\"{string.Empty}\"");
                    try
                    {
                        cancellation.ThrowIfCancellationRequested();
                        if (deleteAfterCopy)
                        {
                            source.Delete(media);
                        }
                    }
                    catch
                    {
                        Trace.WriteLine($"Unable to delete {media.Reference}. Please remove manually.");
                        Logger.WriteLine($"Unable to delete {media.Reference}. Please remove manually.");
                    }
                }
                else
                {
                    Trace.WriteLine($"Unable to parse creation date ({mapping.Tag.Description}) with format {mapping.Mapping.DateTimeParseFormat} for directory {mapping.Mapping.DirectoryName} for {media} Skipping .. ");
                    Logger.WriteLine($"Unable to parse creation date ({mapping.Tag.Description}) with format {mapping.Mapping.DateTimeParseFormat} for directory {mapping.Mapping.DirectoryName} for {media} Skipping .. ");
                    StatusFileWriter.WriteLineWithoutTimeStamp($"\"{media.Reference}\",\"{FileProcessingStatus.Fail}\",\"Unable to parse creation date ({mapping.Tag.Description}) with format {mapping.Mapping.DateTimeParseFormat} for directory {mapping.Mapping.DirectoryName} for {media}\"");
                }
            }
        }
    }
}
