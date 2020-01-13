namespace Sukul.Media.Backup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using CommandLine;
    using SixLabors.ImageSharp.Metadata.Profiles.Exif;
    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
               .WithParsed(RunOptions)
               .WithNotParsed(HandleParseError);

            //if (args.Length > 0)
            //// https://andrewlock.net/using-dependency-injection-in-a-net-core-console-application/
            //{
            //    var tags = ImageHelper.EXIFData(File.ReadAllBytes($"{args[0]}"));

            //    foreach (var tag in tags)
            //    {
            //        Console.WriteLine($"Tag: {tag.Key} | Value: {tag.Value}");
            //    }
            //    Console.ReadLine();
            //}
        }

        static void RunOptions(Options opts)
        {
            //handle options
        }
        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
        }
    }
}
