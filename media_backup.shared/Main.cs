namespace Sukul.Media.Backup.Shared
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class Main
    {
        private IList<string> _filesToProcess = new List<string>();
        private ICollection<Task> _tasks = new Collection<Task>();
        private readonly IMediaProcessor _processor;
        private readonly IMediaDiscovery _discovery;

        public Main(IMediaDiscovery discovery, IMediaProcessor processor)
        {
            this._discovery = discovery;
            this._processor = processor;
        }


        public async void Process(string sourcePath, string destinationPath, bool recursive, bool processImages, bool processVideos, bool deleteAfterCopy)
        {
            this._filesToProcess = await _discovery.List(sourcePath, true, processImages, processVideos);
            foreach (var file in this._filesToProcess)
            {
                this._tasks.Add(Task.Factory.StartNew(() => _processor.Copy(file, destinationPath, deleteAfterCopy)));
            }

            Task.WaitAll(this._tasks.ToArray());
        }

    }
}
