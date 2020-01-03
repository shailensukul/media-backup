namespace Sukul.Media.Backup
{
    using System;
    using System.Threading.Tasks;

    public interface IMediaDestination {
        public Task Save(byte[] data);
        public Task List(string path, bool recursive = false);
    }
}