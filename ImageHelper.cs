namespace Sukul.Media.Backup
{
    using System;
    using System.IO;
    using SixLabors.ImageSharp;

    public class ImageHelper
    {
        // https://github.com/SixLabors/ImageSharp/issues/703
        public static IImageInfo EXIFData(byte[] data) {
            return Image.Identify(new MemoryStream(data));
        }
    }
}