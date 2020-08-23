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

    public sealed class FileSystemDestination2 : IMediaDestination
    {
        private readonly IMediaDestination _mediaProcessor;


        public FileSystemDestination2(IMediaDestination mediaProcessor)
        {}


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
            _mediaProcessor.SaveAsync(desinationFolder, File.ReadAllBytes(filename), Path.GetExtension(filename));
            if (deleteAfterCopy)
            {
                File.Delete(filename);
            }

        }

    }
}
