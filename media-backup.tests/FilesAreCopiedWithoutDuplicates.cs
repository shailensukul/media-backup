using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sukul.Media.Backup.FileSystem;
using Sukul.Media.Backup.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace media_backup.tests
{
    [TestClass]
    public class FilesAreCopiedWithoutDuplicates : SourceFileSpecification
    {
        DateTime start;
        private readonly MultiThreadedFileWriter LogFileWriter = new MultiThreadedFileWriter();
        string currentDir = Directory.GetCurrentDirectory();
        string inputDir;
        string outDir;

        public FilesAreCopiedWithoutDuplicates() 
        {
            this.inputDir = $"{currentDir}\\source-files";
            this.outDir = $"{currentDir}\\destination-files";
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            LogFileWriter.Start($"{assemblyFolder}\\{DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")}-Log.log", CancellationToken.None);
        }

        public override void  Given()
        {
            Directory.CreateDirectory(this.outDir);
            this._discovery = new FileSystemSource();
            this._destination = new FileSystemDestination();
            this._coordinator = new Coordinator<IMediaDiscovery, IMediaDestination>(_discovery, _destination, LogFileWriter);
        }

        public override void When()
        {
            start = DateTime.Now;
            _coordinator.ProcessAsync(inputDir, outDir, true, true, true, false, false, CancellationToken.None);
        }

        [TestMethod]
        public void EnsureFilesAreCopied()
        {
            Assert.AreEqual(Directory.GetFiles(outDir, "*", SearchOption.AllDirectories).Count(), 5, "Expected files to be copied");
            var timeTaken = DateTime.Now.Subtract(start);
            Trace.Write($"Copy took {timeTaken.TotalMilliseconds} milliseconds");
        }
        
        [TestCleanup]
        public void CleanUp()
        {
            Directory.Delete(outDir, true);
        }
    }
}
