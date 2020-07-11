namespace Sukul.Media.Backup.Shared
{
    using Sukul.Media.Backup.Shared;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class Main
    {
        private IList<string> _filesToProcess = new List<string>();
        private ICollection<Task> _tasks = new Collection<Task>();

        public Main(IMediaProcessor processor)
        {
            this._processor = processor;
        }

        private readonly IMediaProcessor _processor;

        public async void Process(string sourcePath, string destinationPath, bool recursive, bool processImages, bool processVideos, bool deleteAfterCopy)
        {
            this._filesToProcess = await _processor.List(sourcePath, true, processImages, processVideos);
            foreach (var file in this._filesToProcess)
            {
                this._tasks.Add(Task.Factory.StartNew(() => this.Copy(file, destinationPath, deleteAfterCopy)));
            }

            Task.WaitAll(this._tasks.ToArray());
        }

        private void Copy(string filename, string topDestinationFolder, bool deleteAfterCopy)
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
            _processor.Save(desinationFolder, File.ReadAllBytes(filename), Path.GetExtension(filename));
            if (deleteAfterCopy)
            {
                File.Delete(filename);
            }

        }
    }
}
