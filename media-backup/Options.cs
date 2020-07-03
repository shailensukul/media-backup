using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sukul.Media.Backup
{
    public class Options
    {
        [Option('s', "source", Required = true, HelpText = "Source path to folder containing files.")]
        public string SourcePath { get; set; }

        [Option('d', "destination", Required = true, HelpText = "Destination path to folder containing files.")]
        public string DestinationPath { get; set; }

        [Option(
            'i',
            "image",
          Default = true,
          HelpText = "Copy all types of image files.")]
        public bool Images { get; set; }

        [Option(
          'v', 
          "video",
          Default = false,
          HelpText = "Copy all types of video files.")]
        public bool Videos { get; set; }

    }
}
