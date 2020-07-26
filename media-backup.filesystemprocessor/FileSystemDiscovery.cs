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
    using System.Text;
    using System.Threading.Tasks;

    public sealed class FileSystemDiscovery : MediaDiscovery
    {
        private readonly IMediaProcessor _mediaProcessor;


        public FileSystemDiscovery(IMediaProcessor mediaProcessor) : base(mediaProcessor)
        {}

        public override async Task<IList<string>> List(string path, bool recursive, bool searchImages, bool searchVideos)
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


        public override void Copy(string filename, string topDestinationFolder, bool deleteAfterCopy)
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
            _mediaProcessor.Save(desinationFolder, File.ReadAllBytes(filename), Path.GetExtension(filename));
            if (deleteAfterCopy)
            {
                File.Delete(filename);
            }

        }

    }
}
