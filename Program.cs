namespace Sukul.Media.Backup
{
    using System;
    using System.IO;
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var info = ImageHelper.EXIFData(File.ReadAllBytes(args[0]));
                Console.WriteLine($"{info.Width}x{info.Height} | BPP: {info.PixelType.BitsPerPixel}");
            }
        }
    }
}
