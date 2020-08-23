using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private readonly List<string> _filesToProcess;
        private readonly ICollection<Task> _tasks;
        private bool _processing = false;

        public Coordinator(S source, D destination)
        {
            _source = source;
            _destination = destination;
            _filesToProcess = new List<string>();
            _tasks = new Collection<Task>();
        }

        public async void ProcessAsync(string sourcePath, bool recursive, bool processImages, bool processVideos, CancellationToken cancellation)
        {
            try
            {
                if (this._processing)
                {
                    throw new ApplicationException("A copy operation is currently in progress. Please wait for processing to finish and try again");
                }

                this._filesToProcess.Clear();
                this._filesToProcess.AddRange(await _source.AcquireAsync(sourcePath, true, processImages, processVideos));

                foreach (var file in this._filesToProcess)
                {
                    this._tasks.Add(Task.Factory.StartNew(() => this._destination.CopyAsync(file, destinationPath, deleteAfterCopy)));
                    cancellation.ThrowIfCancellationRequested();
                }

                Task.WaitAll(this._tasks.ToArray());
                this._processing = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
    }
}
