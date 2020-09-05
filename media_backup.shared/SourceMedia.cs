namespace Sukul.Media.Backup.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Data;
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

        public DateTime CreationDateTime
        {
            get;
            set;
        }

        private SourceMedia()
        {}

        public SourceMedia(byte[] mediaData, string extension, DateTime creationDatetime)
        {
            this.Data = mediaData;
            this.Extension = extension;
            this.CreationDateTime = creationDatetime;
        }
    }
}
