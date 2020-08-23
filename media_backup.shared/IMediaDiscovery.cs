﻿namespace Sukul.Media.Backup.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface IMediaDiscovery
    {
        //void Copy(string filename, string topDestinationFolder, bool deleteAfterCopy);
        IAsyncEnumerable<SourceMedia> AcquireAsync(string path, bool recursive, bool searchImages, bool searchVideos);
    }
}