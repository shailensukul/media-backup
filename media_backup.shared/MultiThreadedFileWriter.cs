namespace Sukul.Media.Backup.Shared
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Threads deliver what to write to the file to the ConcurrentQueue.
    /// A task running in the background will read from the ConcurrentQueue and do the actual file writing.
    /// </summary>
    /// <remarks>
    /// Change to long runnig and pass in
    /// From https://briancaos.wordpress.com/2021/01/12/write-to-file-from-multiple-threads-async-with-c-and-net-core/
    /// </remarks>
    public class MultiThreadedFileWriter
    {
        private ConcurrentQueue<string> _textToWrite;
        private string FileLocation;
        private bool IsStarted;
        public MultiThreadedFileWriter()
        {
            _textToWrite = new ConcurrentQueue<string>();
            IsStarted = false;
        }

        public void Start(string fileLocation, CancellationToken token)
        {
            if (IsStarted)
            {
                throw new ApplicationException("Start cannot be called twice");
            }

            this.FileLocation = fileLocation;
            IsStarted = true;
            // This is the task that will run
            // in the background and do the actual file writing
            Task.Run(() => this.WriteToFile(token));
        }

        public string GetLine()
        {
            if (!IsStarted)
            {
                throw new ApplicationException("Start not called first");
            }

            string cursor = string.Empty;

            if (File.Exists(this.FileLocation))
            {
                using (FileStream fs = new FileStream(this.FileLocation, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        cursor = sr.ReadLine();
                    }
                    fs.Close();
                }
            }

            return cursor;
        }

        public string GetLastLine()
        {
            if (!IsStarted)
            {
                throw new ApplicationException("Start not called first");
            }

            string cursor = string.Empty;
            string lineData = string.Empty;

            if (File.Exists(this.FileLocation))
            {
                using (FileStream fs = new FileStream(this.FileLocation, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        // long offset = sr.BaseStream.Length - ((sr.BaseStream.Length * lengthWeNeed) / 100);
                        // Assuming a line doesnt have more than 500 characters, else use above formula
                        long offset = sr.BaseStream.Length - 500;
                        if (offset < 0)
                        {
                            offset = 0;
                        }

                        // directly move the last 500th position
                        sr.BaseStream.Seek(offset, SeekOrigin.Begin);

                        // From there read lines, not whole file
                        while (!sr.EndOfStream)
                        {
                            lineData = sr.ReadLine();
                            if (sr.Peek() == -1)
                            {
                                cursor = lineData;
                            }
                        }
                    }
                    fs.Close();
                }
            }

            return cursor;
        }

        /// The public method where a thread can ask for a line
        /// to be written.
        public void WriteLine(string line)
        {
            line = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff")}\t{line}";
            if (!IsStarted)
            {
                throw new ApplicationException("Start not called first");
            }

            _textToWrite.Enqueue(line);
        }

        public void WriteLineWithoutTimeStamp(string line)
        {
            if (!IsStarted)
            {
                throw new ApplicationException("Start not called first");
            }

            _textToWrite.Enqueue(line);
        }


        /// <summary>
        /// Read the checkpoiunt file line by line, until any text on the line matches the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsId(string id)
        {
            if (!IsStarted)
            {
                throw new ApplicationException("Start not called first");
            }

            bool idFound = false;

            using (var stream = new FileStream(FileLocation, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    while (sr.Peek() >= 0 && !idFound) // reading the old data
                    {
                        idFound = sr.ReadLine().Contains(id);
                    }
                }
            }

            return idFound;
        }

        /// The actual file writer, running
        /// in the background.
        private async Task WriteToFile(CancellationToken token)
        {
            if (!IsStarted)
            {
                throw new ApplicationException("Start not called first");
            }

            while (!token.IsCancellationRequested)
            {
                using (StreamWriter w = File.AppendText(FileLocation))
                {
                    while (_textToWrite.TryDequeue(out string textLine))
                    {
                        await w.WriteLineAsync(textLine);
                    }
                    w.Flush();
                    await Task.Delay(100);
                }
            }
        }
    }
}
