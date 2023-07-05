using Maggsoft.Core.Entities;
using Maggsoft.Core.Repository;
using Maggsoft.Data.Mongo;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;

namespace Maggsoft.Mongo.Repository
{

    public interface IMongoRepository<T> : IRepository<T> where T : BaseEntity, IEntity
    {
        #region Collection
        IMongoCollection<T> Collection { get; }
        IMongoDatabase Database { get; }
        #endregion

        #region CustomProperty
        /// <summary>
        /// Gets a table
        /// </summary>
        IMongoQueryable<T> Table { get; }

        /// <summary>
        /// Get collection by filter definitions
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IList<T> FindByFilterDefinition(FilterDefinition<T> query);

        #endregion
    }
}
