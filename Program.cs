namespace Sukul.Media.Backup
{
    using System;
    using System.IO;
    using System.Reflection;
    using SixLabors.ImageSharp.Metadata.Profiles.Exif;
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            // https://andrewlock.net/using-dependency-injection-in-a-net-core-console-application/
            {
                var tags = ImageHelper.EXIFData(File.ReadAllBytes($"{args[0]}"));

                foreach (var tag in tags)
                {
                    Console.WriteLine($"Tag: {tag.Key} | Value: {tag.Value}");
                }
                Console.ReadLine();
            }
        }
    }
}
