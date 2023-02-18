using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Maggsoft.Core.Entities;
using Maggsoft.Data.Mongo;
using Maggsoft.Mongo.Extensions;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;

namespace Maggsoft.Mongo.Repository
{
    public sealed class GridFsRepository : IGridFsRepository 
    {
        #region Collection

        /// <summary>
        /// Gets the GridFsBucket
        /// </summary>
        public IGridFSBucket GridFsBucket { get; }

        /// <summary>
        /// Gets the collection
        /// </summary>

        /// <summary>
        ///Gets the Mongo Database
        /// </summary>
        public IMongoDatabase Database { get; }

        #endregion

        #region Ctor

        public GridFsRepository(IMongoDatabase database)
        {
            Database = database;
            GridFsBucket = new GridFSBucket(Database);
        }

        #endregion

        #region Methods

        public async Task<List<GridFSFileInfo>> GetAllAsync()
        {
            return await GridFsBucket.Find(Builders<GridFSFileInfo>.Filter.Empty).ToListAsync();
        }

        public List<GridFSFileInfo<ObjectId>> GetAllFiles()
        {
            var options = new GridFSFindOptions();

            var stream = GridFsBucket.Find(new BsonDocumentFilterDefinition<GridFSFileInfo<ObjectId>>(new BsonDocument()), options).ToList();
            return stream;
        }

        public List<GridFSFileInfo<ObjectId>> GetAllFiles(int skip, int take)
        {
            var options = new GridFSFindOptions {Limit = take, Skip = skip};

            var stream = GridFsBucket.Find(new BsonDocumentFilterDefinition<GridFSFileInfo<ObjectId>>(new BsonDocument()), options).ToList();
            return stream;
        }

        public async Task<IAsyncCursor<GridFSFileInfo>> GetFileById(ObjectId id)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", id);
            return await GridFsBucket.FindAsync(filter);
        }
        public IEnumerable<GridFSFileInfo> GetAllFilesByContentType(string contentType, int skip, int take)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(info => info.Metadata, new BsonDocument(new BsonElement("contentType", contentType)));
            var options = new GridFSFindOptions {Limit = take, Skip = skip};
            var stream = GridFsBucket.Find(filter, options).ToList();
            return stream;
        }

        public async Task<bool> AnyAsync(ObjectId id)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", id);
            return await GridFsBucket.Find(filter).AnyAsync();
        }

        public async Task<bool> AnyAsync(string fileName)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Where(x => x.Filename == fileName);
            return await GridFsBucket.Find(filter).AnyAsync();
        }

        public async Task<GridFSDownloadStream<ObjectId>> DownloadAsync(string fileName)
        {
            return await GridFsBucket.OpenDownloadStreamByNameAsync(fileName);
        }

        public async Task<GridFSDownloadStream<ObjectId>> DownloadAsync(ObjectId id)
        {
            return await GridFsBucket.OpenDownloadStreamAsync(id);
        }

        public async Task DownloadToStreamByNameAsync(string filename, Stream destination)
        {
            var options = new GridFSDownloadByNameOptions {Revision = 0};
            await GridFsBucket.DownloadToStreamByNameAsync(filename, destination, options);
            // or
            using (var stream = await GridFsBucket.OpenDownloadStreamByNameAsync(filename, options))
            {
                // read from stream until end of file is reached
                stream.CopyTo(destination);
                stream.Close();
            }
        }

        public async Task DownloadToStreamAsync(ObjectId id, Stream destination)
        {
            await GridFsBucket.DownloadToStreamAsync(id, destination);
        }

        public async Task DownloadFromStreamAsync(ObjectId id, Stream destination)
        {
            var options = new GridFSDownloadOptions {Seekable = true};
            using (var stream = await GridFsBucket.OpenDownloadStreamAsync(id, options))
            {
                // read from stream until end of file is reached
                stream.CopyTo(destination);
                stream.Close();
            }
        }

        public async Task<byte[]> DownloadAsBytesAsync(ObjectId id)
        {
            var bytes = await GridFsBucket.DownloadAsBytesAsync(id);
            return bytes;
        }

        public async Task<byte[]> DownloadAsBytesByNameAsync(string filename)
        {
            var options = new GridFSDownloadByNameOptions {Revision = 0};
            var bytes = await GridFsBucket.DownloadAsBytesByNameAsync(filename, options);
            return bytes;
        }

        public async Task<string> UploadFileAsync(byte[] source, string filename)
        {
            var options = new GridFSUploadOptions
            {
                // ChunkSizeBytes = 64512, // 63KB
                Metadata = new BsonDocument {{"resolution", "1080P"}, {"copyrighted", true}}
            };

            var id = await GridFsBucket.UploadFromBytesAsync(filename, source, options);
            return id.ToString();
        }

        public async Task<string> UploadFileAsync(Stream source, string filename)
        {
            var options = new GridFSUploadOptions
            {
                //ChunkSizeBytes = 64512, // 63KB
                Metadata = new BsonDocument {{"resolution", "1080P"}, {"copyrighted", true}}
            };

            using (var stream = await GridFsBucket.OpenUploadStreamAsync(filename, options))
            {
                var streamId = stream.Id; // the unique Id of the file being uploaded
                source.CopyTo(stream); // write the contents of the file to stream using synchronous Stream methods
                stream.Close(); // optional because Dispose calls Close

                return streamId.ToString();
            }
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            var options = new GridFSUploadOptions {Metadata = new BsonDocument("contentType", file.ContentType)};
            using (var reader = new StreamReader((Stream) file.OpenReadStream()))
            {
                var stream = reader.BaseStream;
                var fileId = await GridFsBucket.UploadFromStreamAsync(file.FileName, stream, options);
                return fileId.ToString();
            }
        }

        public async Task RenameAsync(string oldFilename, string newFilename)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, oldFilename);
            var filesCursor = await GridFsBucket.FindAsync(filter);
            var files = await filesCursor.ToListAsync();

            foreach (var file in files)
            {
                await GridFsBucket.RenameAsync(file.Id, newFilename);
            }
        }

        public async Task RenameAsync(ObjectId id, string newFilename)
        {
            await GridFsBucket.RenameAsync(id, newFilename);
        }

        public async Task DeleteAsync(string fileName)
        {
            var fileInfo = await GetFileInfoAsync(fileName);
            if (fileInfo != null)
                await DeleteAsync(fileInfo.Id);
        }

        public async Task DeleteAsync(ObjectId id)
        {
            await GridFsBucket.DeleteAsync(id);
        }

        public void Drop()
        {
            GridFsBucket.Drop();
        }

        public async Task DropAsync()
        {
            await GridFsBucket.DropAsync();
        }

        #endregion

        #region Private

        private async Task<GridFSFileInfo> GetFileInfoAsync(string fileName)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename,
                fileName);
            var fileInfo = await
                GridFsBucket.Find(filter).FirstOrDefaultAsync();
            return fileInfo;
        }

        #endregion

        #region Properties

        #endregion
    }
}