namespace Sukul.Media.Backup.FileSystem
{
    using Sukul.Media.Backup.Shared;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class FileSystemSource : IMediaDiscovery
    {
        public async void Delete(SourceMedia media)
        {
            File.Delete(media.Reference);
        }
        public async IAsyncEnumerable<SourceMedia> AcquireAsync(string path, bool recursive, bool searchImages, bool searchVideos)
        {
            SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            Collection<string> filters = new Collection<string>();
            if (searchImages)
            {
                filters.Add(".jpg");
                filters.Add(".jpeg");
                filters.Add(".png");
                filters.Add(".gif");
            }

            if (searchVideos)
            {
                filters.Add(".mp4");
                filters.Add(".avi");
            }


            if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.*", option)
                    .Where(f => filters.Any(f.ToLower().EndsWith))
                    .ToList())
                {
                    yield return new SourceMedia(File.ReadAllBytes(file), Path.GetExtension(file), File.GetCreationTime(file), file);
                }
            }
            else
            {
                throw new ApplicationException($"Path {path} does not exist");
            }
        }
    }
}
