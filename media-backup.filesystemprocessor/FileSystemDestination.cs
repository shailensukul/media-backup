namespace Sukul.Media.Backup.FileSystem
{
    using Sukul.Media.Backup.Shared;
    using System;
    using System.Collections.Concurrent;
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
        // The higher the concurrencyLevel, the higher the theoretical number of operations
        // that could be performed concurrently on the ConcurrentDictionary.  However, global
        // operations like resizing the dictionary take longer as the concurrencyLevel rises.
        // For the purposes of this example, we'll compromise at numCores * 2.
        //int numProcs = Environment.ProcessorCount;
        //int concurrencyLevel = numProcs * 2;

        // We know how many items we want to insert into the ConcurrentDictionary.
        // So set the initial capacity to some prime number above that, to ensure that
        // the ConcurrentDictionary does not need to be resized while initializing it.
        //int NUMITEMS = 64;
        //int initialCapacity = 101;

        ConcurrentDictionary<string, string> ProcessedFileHashes = new ConcurrentDictionary<string, string>((Environment.ProcessorCount * 2), 101);
        public FileSystemDestination()
        { }

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

            ProcessedFileHashes.Concat(Directory.GetFiles(path).Where(f => !ProcessedFileHashes.ContainsKey(f)).Select(f => KeyValuePair.Create<string, string>(f, GetHash(GetFileAsBytes(f)))));

            if (!ProcessedFileHashes.Values.Contains(fileHash))
            {
                //if (!await this.ExistsAsync(path, fileData))
                //{
                // Does the destination already contain the file?
                await File.WriteAllBytesAsync(fileName, fileData);
                ProcessedFileHashes.TryAdd(fileName, fileHash);
                Trace.WriteLine($"Processed file: {fileName}");
                //}
                //else
                //{
                //    Trace.WriteLine($"File {fileName} already exists. Skipping ..");
                //}
            }
            else
            {
                Trace.WriteLine($"Duplicate file found {fileName}. Skipping ...");
            }
            return;
        }

        private byte[] GetFileAsBytes(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    var binaryReader = new BinaryReader(stream);
                    return binaryReader.ReadBytes((int)stream.Length);
                }
            }
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
                        var existingFileHash = Convert.ToBase64String(sha1.ComputeHash(GetFileAsBytes(file)));
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