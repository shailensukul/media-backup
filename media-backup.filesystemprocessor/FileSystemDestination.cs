namespace Sukul.Media.Backup.FileSystem
{
    using Sukul.Media.Backup.Shared;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    public class FileSystemDestination : IMediaDestination
    {
        static List<string> ProcessedFileHashes = new List<string>();
        public FileSystemDestination()
        {}

        private string GetHash(byte[] fileData)
        {
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                return Convert.ToBase64String(sha1.ComputeHash(fileData));
            }
        }

        public async Task SaveAsync(string path, byte[] fileData, string extension)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // ImageHelper.RemoveEXIFData(ref fileData);

            // string destinationFileName = this.GetHash(fileData);
            string destinationFileName = Path.GetRandomFileName();

            destinationFileName = Path.ChangeExtension(destinationFileName, extension);

            var fileHash = GetHash(fileData);
            var fileName = $"{path}\\{destinationFileName}";
            if (!ProcessedFileHashes.Contains(fileHash))
            {
                if (!await this.ExistsAsync(path, fileData))
                {
                    await File.WriteAllBytesAsync(fileName, fileData);
                    ProcessedFileHashes.Add(fileHash);
                } else
                {
                    Trace.WriteLine($"File {fileName} already exists. Skipping ..");
                }
            } else
            {
                Trace.WriteLine($"Duplicate file found {fileName}. Skipping ...");
            }
            return;
        }

        public async Task<bool> ExistsAsync(string path, byte[] fileData)
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

        private async Task<IList<string>> List(string path, bool recursive, bool searchImages, bool searchVideos)
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