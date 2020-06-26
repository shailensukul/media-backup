// Define the TRACE directive, which enables trace output to the
// Trace.Listeners collection. Typically, this directive is defined
// as a compilation argument.
#define TRACE
namespace Sukul.Media.Backup
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using CommandLine;

    class Program
    {
        static IMediaProcessor processor  = new FileSystemProcessor();

        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener(true));
            CommandLine.Parser.Default.ParseArguments<Options>(args)
               .WithParsed(RunOptions)
               .WithNotParsed(HandleParseError);

            //if (args.Length > 0)
            //// https://andrewlock.net/using-dependency-injection-in-a-net-core-console-application/
        }

        static async void RunOptions(Options opts)
        {
            //handle options
            Trace.Indent();
            Trace.WriteLine($"Source: {opts.SourcePath}");
            Trace.WriteLine($"Destination: {opts.DestinationPath}");
            Trace.WriteLine($"Copy images: {opts.Images}");
            Trace.WriteLine($"Copy videos: {opts.Videos}");

            var files = await processor.List(opts.SourcePath, true, opts.Images, opts.Videos);
            foreach (var file in files)
            {
                Trace.WriteLine($"{file}");
            }
            Console.ReadLine();
        }
        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
            foreach (var err in errs)
            {
                Console.WriteLine(err.ToString());
            }
        }
    }
}
