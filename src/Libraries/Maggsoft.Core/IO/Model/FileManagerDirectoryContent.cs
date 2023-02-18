using System;

namespace Maggsoft.Core.IO.Model
{
    public class FileManagerDirectoryContent
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime DateCreated { get; set; }
        public bool HasChild { get; set; }
        public bool IsFile { get; set; }
        public string Type { get; set; }
        public string FilterPath { get; set; }
    }
}
