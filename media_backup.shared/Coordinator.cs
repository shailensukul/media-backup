using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sukul.Media.Backup.Shared
{

    public class Coordinator<S, D> where S : IMediaDiscovery where D : IMediaDestination
    {
        private Coordinator()
        {}

        private readonly S _source;
        private readonly D _destination;

        private readonly ICollection<Task> _tasks;
        private bool _processing = false;

        public Coordinator(S source, D destination)
        {
            _source = source;
            _destination = destination;
            _tasks = new Collection<Task>();
        }

        public async void ProcessAsync(string sourcePath, string destinationPath, bool recursive, bool processImages, bool processVideos, CancellationToken cancellation)
        {
            try
            {
                if (this._processing)
                {
                    throw new ApplicationException("A copy operation is currently in progress. Please wait for processing to finish and try again");
                }

                await foreach (var media in _source.AcquireAsync(sourcePath, true, processImages, processVideos))
                {
                    this._tasks.Add(Task.Factory.StartNew(() =>
                    {
                        DateTime dateTime = default(DateTime);
                        var tags = ImageHelper.EXIFData(media.Data);
                        object date;
                        string desinationFolder;
                        if (tags.TryGetValue("DateTime", out date))
                        {
                            if (DateTime.TryParseExact(Convert.ToString(date), "yyyy:MM:dd HH:mm:ss",
                            CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
                            {
                                {
                                    desinationFolder = $"{destinationPath}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";
                                }
                            }
                        }

                        if (dateTime == default(DateTime))
                        {
                            dateTime = media.CreationDateTime;
                        }
                        desinationFolder = $"{destinationPath}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";
                        Trace.WriteLine($"Copying file t0  {desinationFolder}");
                        this._destination.SaveAsync(desinationFolder, media.Data, media.Extension);
                        cancellation.ThrowIfCancellationRequested();
                    }));

                    cancellation.ThrowIfCancellationRequested();
                    Task.WaitAll(this._tasks.ToArray());
                    this._processing = false;
                }
            }
            catch (Exception ex)
            {
                this._processing = false;
                Trace.WriteLine(ex.ToString());
                throw ex;
            }
        }

        //public async void CopyAsync(string filename, string topDestinationFolder, bool deleteAfterCopy)
        //{
        //    DateTime dateTime = default(DateTime);
        //    byte[] data = File.ReadAllBytes(filename);
        //    var tags = ImageHelper.EXIFData(data);
        //    object date;
        //    string desinationFolder;
        //    if (tags.TryGetValue("DateTime", out date))
        //    {
        //        if (DateTime.TryParseExact(Convert.ToString(date), "yyyy:MM:dd HH:mm:ss",
        //        CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
        //        {
        //            {
        //                desinationFolder = $"{topDestinationFolder}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";
        //            }
        //        }
        //    }
        //    if (dateTime == default(DateTime))
        //    {
        //        dateTime = File.GetCreationTime(filename);
        //    }
        //    desinationFolder = $"{topDestinationFolder}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";

        //    Trace.WriteLine($"{filename}");
        //    await this.SaveAsync(desinationFolder, File.ReadAllBytes(filename), Path.GetExtension(filename));
        //    if (deleteAfterCopy)
        //    {
        //        File.Delete(filename);
        //    }
        //}
    }
}
