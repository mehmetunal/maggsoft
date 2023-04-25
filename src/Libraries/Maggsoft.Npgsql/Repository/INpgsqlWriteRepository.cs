using Maggsoft.Core.Entities;
using Maggsoft.Core.Repository;
using Maggsoft.Data.Npgsql;

namespace Maggsoft.Npgsql.Repository
{
    public interface INpgsqlWriteRepository<T> : INpgsqlRepository<T>, IWriteRepository<T> where T : BaseEntity, IEntity
    {

    }
}
