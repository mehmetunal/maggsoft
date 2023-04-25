using Maggsoft.Core.Entities;
using Maggsoft.Core.Repository;
using Maggsoft.Data.Mongo;

namespace Maggsoft.Mongo.Repository
{
    public interface IMongoReadRepository<T> : IMongoRepository<T>, IReadRepository<T> where T : BaseEntity, IEntity
    {

    }
}
