namespace Sukul.Media.Backup.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    public sealed class SourceMedia
    {
        public string Reference
        {
            get;
            set;
        }

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

        public SourceMedia(byte[] mediaData, string extension, DateTime creationDatetime, string reference)
        {
            this.Data = mediaData;
            this.Extension = extension;
            this.CreationDateTime = creationDatetime;
            this.Reference = reference;
        }
    }
}
