namespace Sukul.Media.Backup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    public class FileSystemDestination : IMediaDestination
    {
        public async Task<bool> Exists(string path, byte[] fileData)
        {
            if (Directory.Exists(path))
            {
                string copyFilehash;
                using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
                {
                    copyFilehash = Convert.ToBase64String(sha1.ComputeHash(fileData));

                    foreach (var file in Directory.GetFiles(path))
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

        public async Task Save(string path, string fileName, byte[] fileData)
        {
            if (Directory.Exists(path))
            {
                await File.WriteAllBytesAsync(path, fileData);
                return;
            }
            throw new ApplicationException($"Path {path} does not exist");
        }

        public async Task<IList<string>> List(string path, bool recursive = false)
        {
            if (Directory.Exists(path))
            {
                return new List<string>(Directory.GetFiles(path));
            }
            throw new ApplicationException($"Path {path} does not exist");
        }
    }
}