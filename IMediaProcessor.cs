namespace Sukul.Media.Backup
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMediaProcessor {

        public Task<bool> Exists(string path, byte[] fileData);
        public Task Save(string path, byte[] fileData, string extension);
        public Task<IList<string>> List(string path, bool recursive, bool searchImages, bool searchVideos);
    }
}