using System;
using System.Collections.Generic;

namespace Mediah.Models
{
    public class SeperateFile
    {
        public string Filename { get; set; }
        public List<SeperateFileFilePart> Parts { get; set; }
        public long Timestamp { get; set; }
        public int UploadedSize { get; set; }
    }

    public class SeperateFileFilePart
    {
        public int Size { get; set; }
        public string Url { get; set; }
    }
}