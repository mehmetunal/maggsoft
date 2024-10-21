using Maggsoft.Core.Entities;
using Maggsoft.Core.Repository;
using Maggsoft.Data.Sqlite;
using System.Linq;
using System.Threading.Tasks;

namespace Maggsoft.Sqlite.Repository;

public interface ISqliteRepository<T> : IRepository<T> where T : BaseEntity, IEntity 
{
    #region CustomMethod
    IQueryable<T> AsNoTrackingWithIdentityResolution();
    IQueryable<T> FromSqlRaw(string sql, params object[] par);
    int Execute(string sql, params object[] par);
    Task<int> ExecuteAsync(string sql, params object[] par);
    IQueryable<T> Table { get; }
    #endregion

    #region SaveChange
    int SaveChanges();
    Task<int> SaveChangesAsync();
    #endregion
}