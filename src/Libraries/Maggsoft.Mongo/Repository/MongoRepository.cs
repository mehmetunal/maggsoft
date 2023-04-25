using Maggsoft.Core.Entities;
using Maggsoft.Data.Mongo;
using Maggsoft.Mongo.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Maggsoft.Mongo.Repository
{
    public class MongoRepository<T> : IMongoRepository<T> where T : BaseEntity, IEntity
    {
        #region Fields

        /// <summary>
        /// Gets the collection
        /// </summary>
        public IMongoCollection<T> Collection { get; }
        /// <summary>
        /// Mongo Database
        /// </summary>
        public IMongoDatabase Database { get; }

        #endregion

        #region Ctor

        public MongoRepository(IMongoDatabase database)
        {
            Database = database;
            Collection = Database.GetCollection<T>(typeof(T).GetCollectionName());
        }

        #endregion

        #region Properties

        public IMongoQueryable<T> Table => Collection.AsQueryable();

        public IList<T> FindByFilterDefinition(FilterDefinition<T> query)
            => Collection.Find(query).ToList();

        #endregion
    }
}