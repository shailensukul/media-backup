namespace Sukul.Media.Backup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Metadata.Profiles.Exif;

    public class ImageHelper
    {
        // https://github.com/SixLabors/ImageSharp/issues/703
        public static Dictionary<string, object> EXIFData(byte[] data) {
            var info = Image.Identify(new MemoryStream(data));
            Dictionary<string, object> tags = new Dictionary<string, object>();
            foreach (var t in info.Metadata?.ExifProfile?.Values)
            {
                tags.Add(t.Tag.ToString(), t.Value);
            }
            return tags;
        }
    }
}