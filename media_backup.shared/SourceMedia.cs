namespace Sukul.Media.Backup.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public sealed class SourceMedia
    {
        public string Extension
        {
            get;
            private set;
        }

        public byte[] Data
        {
            get;
            private set;
        }

        private SourceMedia()
        {}

        public SourceMedia(byte[] mediaData, string extension)
        {
            this.Data = Data;
            this.Extension = extension;
        }
    }
}
