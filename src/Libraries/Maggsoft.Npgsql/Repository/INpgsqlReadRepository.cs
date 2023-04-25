using Maggsoft.Core.Entities;
using Maggsoft.Core.Repository;
using Maggsoft.Data.Npgsql;

namespace Maggsoft.Npgsql.Repository
{
    public interface INpgsqlReadRepository<T> : INpgsqlRepository<T>, IReadRepository<T> where T : BaseEntity, IEntity
    {

    }
}
