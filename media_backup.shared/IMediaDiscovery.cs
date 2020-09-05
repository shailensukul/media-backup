namespace Sukul.Media.Backup.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface IMediaDiscovery
    {
        public IAsyncEnumerable<SourceMedia> AcquireAsync(string path, bool recursive, bool searchImages, bool searchVideos);

        public void Delete(SourceMedia media);
    }
}
