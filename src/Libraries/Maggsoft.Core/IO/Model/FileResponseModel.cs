using System.IO;

namespace Maggsoft.Core.IO.Model
{
    public class FileResponseModel
    {
        public FileInfo FileInfo { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public bool IsFolder { get; set; }
        public string Extension => FileInfo.Extension.Trim('.');
        public bool IsSuccess { get; set; }
    }
}
