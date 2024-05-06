using System;
using System.Collections.Generic;

namespace Mediah.Models
{
    public class File
    {
        public string Filename { get; set; }
        public int Id { get; set; }
        public List<FilePart> Parts { get; set; }
        public long Timestamp { get; set; }
        public int UploadedSize { get; set; }
    }

    public class FilePart
    {
        public int Size { get; set; }
        public string Url { get; set; }
    }
}