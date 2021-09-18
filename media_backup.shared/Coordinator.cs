using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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

        public static async Task CopyItemToDestination(SourceMedia media, string destinationPath, S source, D destination, CancellationToken cancellation)
        {
            DateTime dateTime = default(DateTime);
            var tags = ImageHelper.EXIFData(media.Data);
            object date;
            string desinationFolder;

            if (tags != null && tags.TryGetValue("ModifyDate", out date))
            {
                if (DateTime.TryParseExact(Convert.ToString(date), "yyyy:MM:dd HH:mm:ss",
                CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
                {
                    {
                        desinationFolder = $"{destinationPath}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";
                    }
                }
            }
            else
            {
                if (tags != null && tags.TryGetValue("DateTime", out date))
                {
                    if (DateTime.TryParseExact(Convert.ToString(date), "yyyy:MM:dd HH:mm:ss",
                    CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
                    {
                        {
                            desinationFolder = $"{destinationPath}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";
                        }
                    }
                }
            }

            if (dateTime == default(DateTime))
            {
                dateTime = media.CreationDateTime;
            }

            desinationFolder = $"{destinationPath}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";

            cancellation.ThrowIfCancellationRequested();

            // Check whether the file already exists in destination folder


            Trace.WriteLine($"Copying file to  {desinationFolder}");
            await destination.SaveAsync(desinationFolder, media.Data, media.Extension);
            try
            {
                cancellation.ThrowIfCancellationRequested();
                source.Delete(media);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Unable to delete {media}. Please remove manually.");
            }
        }
    }
}
