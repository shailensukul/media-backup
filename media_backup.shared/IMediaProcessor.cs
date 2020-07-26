namespace Sukul.Media.Backup.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMediaProcessor {

        Task<bool> Exists(string path, byte[] fileData);
        public Task Save(string path, byte[] fileData, string extension);
        public void Copy(string filename, string topDestinationFolder, bool deleteAfterCopy);
    }
}