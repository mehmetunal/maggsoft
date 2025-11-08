using System.Linq;
using System.Threading.Tasks;
using Maggsoft.Core.Entities;
using Maggsoft.Core.Repository;
using Maggsoft.Data.Mysql;

namespace Maggsoft.Mysql.Repository;

public interface IMysqlRepository<T> : IRepository<T> where T : BaseEntity, IEntity
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