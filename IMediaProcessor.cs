namespace Sukul.Media.Backup
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMediaProcessor {

        public Task<bool> Exists(string path, byte[] fileData);
        public Task Save(string path, string fileName, byte[] fileData);
        public Task<IList<string>> List(string path, bool recursive, bool searchImages, bool searchVideos);
    }
}