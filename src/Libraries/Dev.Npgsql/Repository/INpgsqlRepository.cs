using Dev.Core.Entities;
using Dev.Core.Repository;
using Dev.Data.Npgsql;
using System.Linq;
using System.Threading.Tasks;

namespace Dev.Npgsql.Repository
{
    public interface INpgsqlRepository<T> : IRepository<T> where T : BaseEntity, IEntity
    {
        #region CustomMethod
        IQueryable<T> AsNoTrackingWithIdentityResolution();
        IQueryable<T> FromSqlRaw(string sql, params object[] par);
        int Execute(string sql, params object[] par);
        Task<int> ExecuteAsync(string sql, params object[] par);
        IQueryable<T> Table { get; }
        #endregion
    }
}
