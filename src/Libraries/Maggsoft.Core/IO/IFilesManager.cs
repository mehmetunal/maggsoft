using System.IO;
using Maggsoft.Core.IO.Model;
using Microsoft.AspNetCore.Http;

namespace Maggsoft.Core.IO;

public interface IFilesManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    void Delete(string path);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    DirectoryInfo FolderCreate(string path);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="newFolderName"></param>
    /// <returns></returns>
    string FolderEditName(string path, string newFolderName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="createdPath"></param>
    /// <param name="file"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    FileResponseModel FilesCreate(string createdPath, IFormFile file, string fileName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="file"></param>
    /// <param name="newFileName"></param>
    /// <returns></returns>
    FileResponseModel FilesEditName(string path, IFormFile file, string newFileName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="newFileName"></param>
    /// <returns></returns>
    FileInfo FileRename(string path, string newFileName);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    FileInfo FileRandomName(string path);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="directoryInfo"></param>
    /// <returns></returns>
    string GetFolderType(DirectoryInfo directoryInfo);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    string FormatFileSize(long bytes);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newPath"></param>
    /// <param name="filePath"></param>
    void MergeChunks(string newPath, string filePath);

    /// <summary>
    /// convert IFormFile to bytes
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    byte[] GetByteImage(IFormFile file);
}