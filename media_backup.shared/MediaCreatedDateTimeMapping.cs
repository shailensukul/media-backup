using System;
using System.Collections.Generic;
using System.Text;

namespace Sukul.Media.Backup.Shared
{
    /// <summary>
    /// Maps the Created datetime to the format it is saved in
    /// </summary>
    public sealed class MediaCreatedDateTimeMapping
    {
        public string DirectoryName { get; set; }
        public string Name { get; set; }
        public string DateTimeParseFormat { get; set; }
    }
}
