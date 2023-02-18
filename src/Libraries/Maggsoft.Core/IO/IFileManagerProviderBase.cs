using Maggsoft.Core.IO.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;

namespace Maggsoft.Core.IO
{
    public interface IFileManagerProviderBase
    {
        //public string ContentRootPath;
        FileManagerResponse GetFiles(string path, bool showHiddenItems, params object[] data);
        IEnumerable<FileManagerDirectoryContent> ReadFiles(DirectoryInfo directory, string[] extensions, bool showHiddenItems, params object[] data);
        IEnumerable<FileManagerDirectoryContent> ReadDirectories(DirectoryInfo directory, string[] extensions, bool showHiddenItems, params object[] data);
        FileManagerResponse CreateFolder(string path, string name, params object[] data);
        FileManagerResponse GetDetails(string path, string[] names, params object[] data);
        FileManagerResponse Remove(string path, string[] names, params object[] data);
        FileManagerResponse Rename(string path, string name, string newName, bool replace = false, params object[] data);
        FileManagerResponse CopyTo(string path, string targetPath, string[] names, string[] replacedItemNames, params object[] data);
        FileManagerResponse MoveTo(string path, string targetPath, string[] names, string[] replacedItemNames, params object[] data);
        FileManagerResponse Search(string path, string searchString, bool showHiddenItems, bool caseSensitive, params object[] data);
        string byteConversion(long fileSize);
        string WildcardToRegex(string pattern);
        FileStreamResult Download(string path, string[] names, params object[] data);
        FileManagerResponse Upload(string path, IList<IFormFile> uploadFiles, string action, string[] replacedItemNames, params object[] data);
        FileStreamResult GetImage(string path, bool allowCompress, params object[] data);
        FileStreamResult DownloadFile(string path, string[] names = null);
        FileStreamResult DownloadZip(string path, string[] names = null, string zipName = "file.zip");
        void DeleteDirectory(string path);
        FileManagerDirectoryContent GetFileDetails(string path);
        string ToCamelCase(FileManagerResponse userData);
    }
}
