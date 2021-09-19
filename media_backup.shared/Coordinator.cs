using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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

        private static Collection<MediaCreatedDateTimeMapping> CreatedDateTimeMappings = new Collection<MediaCreatedDateTimeMapping>()
        {
            new MediaCreatedDateTimeMapping { DirectoryName="QuickTime Movie Header", Name="Created", DateTimeParseFormat = "ddd MMM dd hh:mm:ss yyyy" },
            new MediaCreatedDateTimeMapping { DirectoryName="Exif IFD0", Name="Date/Time", DateTimeParseFormat = "yyyy:MM:dd HH:mm:ss" }
        };

        public Coordinator(S source, D destination)
        {
            _source = source;
            _destination = destination;
            _tasks = new Collection<Task>();
        }

        public async void ProcessAsync(string sourcePath, string destinationPath, bool recursive, bool processImages, bool processVideos,
            CancellationToken cancellation)
        {
            try
            {
                var options = new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 8,
                };
                var search = new ActionBlock<SourceMedia>((media) => CopyItemToDestination(media, destinationPath, _source, _destination, cancellation), options);

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

        public static async Task CopyItemToDestination(SourceMedia media, string destinationPath, S source, D destination, CancellationToken cancellation)
        {
            DateTime dateTime = default(DateTime);

            IEnumerable<MetadataExtractor.Tag> tags;
            using (var stream = new MemoryStream(media.Data))
            {
                tags = ImageMetadataReader.ReadMetadata(stream).SelectMany(d => d.Tags);
            }

            string desinationFolder;

            var mappings = from m in CreatedDateTimeMappings
                           join t in tags
                           on new { m.DirectoryName, m.Name } equals new { t.DirectoryName, t.Name }
                           select (Mapping: m, Tag: t);

            if (!mappings.Any())
            {
                Trace.WriteLine($"Unable to determine creation date for {media} Skipping .. ");
            }
            else
            {
                var mapping = mappings.First();
                if (DateTime.TryParseExact(Convert.ToString(mapping.Tag.Description), mapping.Mapping.DateTimeParseFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
                {
                    desinationFolder = $"{destinationPath}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";

                    //desinationFolder = $"{destinationPath}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";

                    cancellation.ThrowIfCancellationRequested();

                    Trace.WriteLine($"Copying file to  {desinationFolder}");
                    await destination.SaveAsync(desinationFolder, media.Data, media.Extension);
                    try
                    {
                        cancellation.ThrowIfCancellationRequested();
                        source.Delete(media);
                    }
                    catch
                    {
                        Trace.WriteLine($"Unable to delete {media}. Please remove manually.");
                    }
                }
                else
                {
                    Trace.WriteLine($"Unable to parse creation date ({mapping.Tag.Description}) with format {mapping.Mapping.DateTimeParseFormat} for {media} Skipping .. ");
                }
            }
        }
    }
}
