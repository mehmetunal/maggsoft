using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Maggsoft.Core.Entities;
using Maggsoft.Core.Repository;
using Maggsoft.Data.Mongo;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;

namespace Maggsoft.Mongo.Repository
{
    public interface IGridFsRepository
    {
        #region Collection

        public IGridFSBucket GridFsBucket { get; }
        IMongoDatabase Database { get; }

        #endregion

        #region CustomProperty

        #endregion

        #region GET

        Task<List<GridFSFileInfo>> GetAllAsync();
        List<GridFSFileInfo<ObjectId>> GetAllFiles();
        List<GridFSFileInfo<ObjectId>> GetAllFiles(int skip, int take);
        IEnumerable<GridFSFileInfo> GetAllFilesByContentType(string contentType, int skip, int take);
        Task<IAsyncCursor<GridFSFileInfo>> GetFileById(ObjectId id);
        #endregion

        #region ANY

        Task<bool> AnyAsync(ObjectId id);
        Task<bool> AnyAsync(string fileName);

        #endregion

        #region DOWNLOAD

        Task<GridFSDownloadStream<ObjectId>> DownloadAsync(string fileName);
        Task<GridFSDownloadStream<ObjectId>> DownloadAsync(ObjectId id);
        Task DownloadToStreamByNameAsync(string filename, Stream destination);
        Task DownloadToStreamAsync(ObjectId id, Stream destination);
        Task DownloadFromStreamAsync(ObjectId id, Stream destination);
        Task<byte[]> DownloadAsBytesAsync(ObjectId id);
        Task<byte[]> DownloadAsBytesByNameAsync(string filename);

        #endregion

        #region UPLOAD

        Task<string> UploadFileAsync(byte[] source, string filename);
        Task<string>  UploadFileAsync(Stream source, string filename);
        Task<string> UploadAsync(IFormFile file);
        Task RenameAsync(string oldFilename, string newFilename);
        Task RenameAsync(ObjectId id, string newFilename);

        #endregion

        #region DELETE

        Task DeleteAsync(string fileName);
        Task DeleteAsync(ObjectId id);
        void Drop();
        Task DropAsync();

        #endregion
    }
}