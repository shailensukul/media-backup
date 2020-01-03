namespace Sukul.Media.Backup
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class FileSystemDestination : IMediaDestination
    {
         public async Task Save(byte[] data) {
             throw new NotImplementedException();
         }
        public async Task List(string path, bool recursive = false) {
            throw new NotImplementedException();
        }
    }
}