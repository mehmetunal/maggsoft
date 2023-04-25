using Maggsoft.Core.Entities;
using Maggsoft.Core.Repository;
using Maggsoft.Data.Mongo;

namespace Maggsoft.Mongo.Repository
{
    public interface IMongoWriteRepository<T> : IMongoRepository<T>, IWriteRepository<T> where T : BaseEntity, IEntity
    {

    }
}
