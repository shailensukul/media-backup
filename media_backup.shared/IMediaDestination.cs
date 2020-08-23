namespace Sukul.Media.Backup.Shared
{
    using System.Threading.Tasks;

    public interface IMediaDestination {

        Task<bool> ExistsAsync(string path, byte[] fileData);
        public Task SaveAsync(string path, byte[] fileData, string extension);
    }
}