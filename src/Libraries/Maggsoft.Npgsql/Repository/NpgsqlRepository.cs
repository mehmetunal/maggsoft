using Maggsoft.Core.Entities;
using Maggsoft.Data.Npgsql;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Maggsoft.Npgsql.Repository
{
    public class NpgsqlRepository<T> : INpgsqlRepository<T> where T : BaseEntity, IEntity
    {
        #region Variables

        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        #endregion

        #region Constructor

        public NpgsqlRepository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        #endregion


        #region Method

        public IQueryable<T> AsNoTrackingWithIdentityResolution()
            => _dbSet.AsNoTrackingWithIdentityResolution();

        public IQueryable<T> FromSqlRaw(string sql, params object[] par)
            => _dbSet.FromSqlRaw(sql, par);

        public int Execute(string sql, params object[] par)
            => _context.Database.ExecuteSqlRaw(sql, par);

        public async Task<int> ExecuteAsync(string sql, params object[] par)
            => await _context.Database.ExecuteSqlRawAsync(sql, par);

        public IQueryable<T> Table => _dbSet.AsQueryable();

        #endregion
    }
}