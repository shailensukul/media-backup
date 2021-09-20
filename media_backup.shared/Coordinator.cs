using MetadataExtractor;
using System;
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

        private readonly ICollection<Task> _tasks;

        private enum FileProcessingStatus
        {
            Success, Fail
        }

        private static Collection<(string FullFilename, FileProcessingStatus status, string reason)> FileProcessingResult;

        private static Collection<MediaCreatedDateTimeMapping> CreatedDateTimeMappings = new Collection<MediaCreatedDateTimeMapping>()
        {
            new MediaCreatedDateTimeMapping { DirectoryName="QuickTime Movie Header", Name="Created", DateTimeParseFormat = "ddd MMM dd HH:mm:ss yyyy" },
            new MediaCreatedDateTimeMapping { DirectoryName="Exif IFD0", Name="Date/Time", DateTimeParseFormat = "yyyy:MM:dd HH:mm:ss" }
        };

        public Coordinator(S source, D destination)
        {
            _source = source;
            _destination = destination;
            _tasks = new Collection<Task>();
        }

        public async void ProcessAsync(string sourcePath, string destinationPath, bool recursive, bool processImages, bool processVideos, bool deleteAfterCopy, bool whatIf,
            CancellationToken cancellation)
        {
            try
            {
                FileProcessingResult = new Collection<(string FullFilename, FileProcessingStatus status, string reason)>();

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
                throw;
            }
            finally
            {
                var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                WriteToCSV($"{assemblyFolder}\\{DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")}-Processing-Result.csv");
            }
        }

        private static void WriteToCSV(string fullFilename)
        {
            try
            {
                File.WriteAllLines(fullFilename, new List<string> { "Media,Status,Reason" }.Concat(FileProcessingResult.Select(l => $"{l.FullFilename},{l.status.ToString()},{l.reason}")));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        private static DateTime ParseDateTime(string mediaDateTime, DateTime fr)
        {
            DateTime dateTime = default(DateTime);

            if (DateTime.TryParseExact(Convert.ToString(mediaDateTime), "ddd MMM dd hh:mm:ss yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
            {
            }
            else if (DateTime.TryParseExact(Convert.ToString(mediaDateTime), "yyyy: MM:dd HH: mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
            {
            }

            return dateTime;
        }

        public static async Task CopyItemToDestination(SourceMedia media, string destinationPath, S source, D destination, bool deleteAfterCopy, bool whatIf, CancellationToken cancellation)
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
                    FileProcessingResult.Add((media.Reference, FileProcessingStatus.Fail, $"{ex.ToString()}"));
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
                FileProcessingResult.Add((media.Reference, FileProcessingStatus.Fail, $"Unable to determine creation date for {media.Reference}"));
            }
            else
            {
                var mapping = mappings.First();
                if (DateTime.TryParseExact(Convert.ToString(mapping.Tag.Description), mapping.Mapping.DateTimeParseFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
                {
                    desinationFolder = $"{destinationPath}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";

                    cancellation.ThrowIfCancellationRequested();

                    Trace.WriteLine($"Copying file to  {desinationFolder}");
                    if (!whatIf)
                    {
                        await destination.SaveAsync(desinationFolder, media.Data, media.Extension);
                    }
                    FileProcessingResult.Add((media.Reference, FileProcessingStatus.Success, string.Empty));
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
                    }
                }
                else
                {
                    Trace.WriteLine($"Unable to parse creation date ({mapping.Tag.Description}) with format {mapping.Mapping.DateTimeParseFormat} for directory {mapping.Mapping.DirectoryName} for {media} Skipping .. ");
                    FileProcessingResult.Add((media.Reference, FileProcessingStatus.Fail, $"Unable to parse creation date ({mapping.Tag.Description}) with format {mapping.Mapping.DateTimeParseFormat} for directory {mapping.Mapping.DirectoryName} for {media}"));
                }
            }
        }
    }
}
