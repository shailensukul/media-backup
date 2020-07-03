namespace Sukul.Media.Backup
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using SixLabors.ImageSharp.Metadata.Profiles.Exif;

    public class FileSystemProcessor : IMediaProcessor
    {
        public async Task<bool> Exists(string path, byte[] fileData)
        {
            if (Directory.Exists(path))
            {
                string copyFilehash;
                using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
                {
                    copyFilehash = Convert.ToBase64String(sha1.ComputeHash(fileData));

                    foreach (var file in await this.List(path, false, true, true))
                    {
                        var existingFileHash = Convert.ToBase64String(sha1.ComputeHash(File.ReadAllBytes(file)));
                        if (copyFilehash == existingFileHash)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private string GetHash(byte[] fileData)
        {
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                return Convert.ToBase64String(sha1.ComputeHash(fileData));
            }
        }

        public async Task Save(string path, byte[] fileData, string extension)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // string destinationFileName = this.GetHash(fileData);
            string destinationFileName = Path.GetRandomFileName();

            destinationFileName = Path.ChangeExtension(destinationFileName, extension);

            if (!await this.Exists(path, fileData))
            {
                await File.WriteAllBytesAsync($"{path}\\{destinationFileName}", fileData);
            }
            return;
        }

        public async Task<IList<string>> List(string path, bool recursive, bool searchImages, bool searchVideos)
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
                return Directory.EnumerateFiles(path, "*.*", option)
                    .Where(f => filters.Any(f.ToLower().EndsWith))
                    .ToList();
            }
            throw new ApplicationException($"Path {path} does not exist");
        }
    }
}