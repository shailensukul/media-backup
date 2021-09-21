// Define the TRACE directive, which enables trace output to the
// Trace.Listeners collection. Typically, this directive is defined
// as a compilation argument.
#define TRACE
namespace Sukul.Media.Backup
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLine;
    using Sukul.Media.Backup.FileSystem;
    using Sukul.Media.Backup.Shared;

    class Program
    {

        static Coordinator<IMediaDiscovery, IMediaDestination> _coordinator;
        private static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private static readonly MultiThreadedFileWriter LogFileWriter = new MultiThreadedFileWriter();

        private enum ExitCode : int
        {
            Success = 0,
            UnknownError = 10
        }

        static void Main(string[] args)
        {
            try
            {
                var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                LogFileWriter.Start($"{assemblyFolder}\\{DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")}-Log.log", CancellationTokenSource.Token);
                _coordinator = new Coordinator<IMediaDiscovery, IMediaDestination>(new FileSystemSource(), new FileSystemDestination(), LogFileWriter);
                ConsoleTraceListener listener = new ConsoleTraceListener();
                Trace.Listeners.Add(listener);

                CommandLine.Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(RunOptions)
                   .WithNotParsed(HandleParseError);
            }
            catch (Exception ex)
            {
                CancellationTokenSource.Cancel();
                Trace.WriteLine(ex.ToString());
                LogFileWriter.WriteLine(ex.ToString());

                Task.Delay(200);
                Environment.Exit((int)ExitCode.UnknownError);
            }

            Task.Delay(200);
            Environment.Exit((int)ExitCode.Success);
        }

        static void RunOptions(Options opts)
        {
            //handle options
            Trace.Indent();
            Trace.WriteLine($"Source: {opts.SourcePath}");
            Trace.WriteLine($"Destination: {opts.DestinationPath}");
            Trace.WriteLine($"Copy images: {opts.Images}");
            Trace.WriteLine($"Copy videos: {opts.Videos}");
            Trace.WriteLine($"What If: {opts.WhatIf}");
            Trace.WriteLine($"Remove files after copying: {opts.DeleteAfterCopy}");

            _coordinator.ProcessAsync(opts.SourcePath, opts.DestinationPath, true, opts.Images, opts.Videos, opts.DeleteAfterCopy, opts.WhatIf, CancellationTokenSource.Token);

            Console.WriteLine("Finished. Press ENTER to exit");
            Console.ReadLine();
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
            foreach (var err in errs)
            {
                Console.WriteLine(err.ToString());
                LogFileWriter.WriteLine(err.ToString());
            }
        }
    }
}
