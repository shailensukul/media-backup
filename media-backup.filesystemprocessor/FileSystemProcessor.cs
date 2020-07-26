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

    public class FileSystemProcessor : IMediaProcessor
    {
        List<string> ProcessedFileHashes = new List<string>();
        public FileSystemProcessor()
        {}

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

            // ImageHelper.RemoveEXIFData(ref fileData);

            // string destinationFileName = this.GetHash(fileData);
            string destinationFileName = Path.GetRandomFileName();

            destinationFileName = Path.ChangeExtension(destinationFileName, extension);

            var fileHash = GetHash(fileData);
            var fileName = $"{path}\\{destinationFileName}";
            if (!ProcessedFileHashes.Contains(fileHash))
            {
                if (!await this.Exists(path, fileData))
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

        public async void Copy(string filename, string topDestinationFolder, bool deleteAfterCopy)
        {
            DateTime dateTime = default(DateTime);
            byte[] data = File.ReadAllBytes(filename);
            var tags = ImageHelper.EXIFData(data);
            object date;
            string desinationFolder;
            if (tags.TryGetValue("DateTime", out date))
            {
                if (DateTime.TryParseExact(Convert.ToString(date), "yyyy:MM:dd HH:mm:ss",
                CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
                {
                    {
                        desinationFolder = $"{topDestinationFolder}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";
                    }
                }
            }
            if (dateTime == default(DateTime))
            {
                dateTime = File.GetCreationTime(filename);
            }
            desinationFolder = $"{topDestinationFolder}\\{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";

            Trace.WriteLine($"{filename}");
            await this.Save(desinationFolder, File.ReadAllBytes(filename), Path.GetExtension(filename));
            if (deleteAfterCopy)
            {
                File.Delete(filename);
            }
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