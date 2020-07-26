namespace Sukul.Media.Backup.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class MediaDiscovery : IMediaDiscovery
    {
        IMediaProcessor _mediaProcessor;
        public MediaDiscovery(IMediaProcessor processor)
        {
            _mediaProcessor = processor;
        }
        public abstract void Copy(string filename, string topDestinationFolder, bool deleteAfterCopy);
        public abstract Task<IList<string>> List(string path, bool recursive, bool searchImages, bool searchVideos);
    }
}
