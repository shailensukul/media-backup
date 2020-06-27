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
    using CommandLine;

    class Program
    {
        static IMediaProcessor processor = new FileSystemProcessor();

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
                Copy(file);
                Trace.WriteLine($"{file}");
            }
            Console.ReadLine();
        }

        static void Copy(string filename)
        {
            DateTime dateTime;
            byte[] data = File.ReadAllBytes(filename);
            var tags = ImageHelper.EXIFData(data);
            object date;
            string desinationFolder;
            if (tags.TryGetValue("DateTime", out date))
            {
                DateTime.TryParseExact(Convert.ToString(date), "yyyy:MM:dd HH:mm:ss",
                CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime);
                {
                    desinationFolder = $"{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";
                }
            }
            dateTime = File.GetCreationTime(filename);
            desinationFolder= $"{dateTime.Year}\\{dateTime.Month.ToString().PadLeft(2, '0')}\\{dateTime.Day.ToString().PadLeft(2, '0')}";
            processor.Save(desinationFolder, File.ReadAllBytes(filename));

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
