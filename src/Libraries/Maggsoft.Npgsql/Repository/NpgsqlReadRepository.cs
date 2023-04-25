using System;
using System.Linq;
using Maggsoft.Data.Npgsql;
using Maggsoft.Core.Entities;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Maggsoft.Npgsql.Repository
{
    public class NpgsqlReadRepository<T> : NpgsqlRepository<T>, INpgsqlReadRepository<T> where T : BaseEntity, IEntity
    {
        #region Variables

        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        #endregion

        #region Constructor

        public NpgsqlReadRepository(DbContext context)
            : base(context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        #endregion

        #region Methos

        public virtual IQueryable<T> Get()
            => _dbSet;

        public virtual async Task<IEnumerable<T>> GetAsync()
            => await _dbSet.ToListAsync();

        public virtual IEnumerable<T> FindAll(Expression<Func<T, bool>> @where)
            => _dbSet.Where(where);

        public virtual IList<T> FindAll(Func<IQueryable<T>, IQueryable<T>> func = null)
        {
            IList<T> FindAll()
            {
                var query = func != null ? func(_dbSet) : _dbSet;
                return query.ToList();
            }

            return FindAll();
        }

        public virtual async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> @where)
            => await _dbSet.Where(where).ToListAsync();

        public virtual async Task<IList<T>> FindAllAsync(Func<IQueryable<T>, IQueryable<T>> func = null)
        {
            Task<List<T>> FindAllAsync()
            {
                var query = func != null ? func(_dbSet) : _dbSet;
                return query.ToListAsync();
            }

            return await FindAllAsync();
        }
        public virtual T Find(Expression<Func<T, bool>> @where)
            => _dbSet.FirstOrDefault(where);

        public virtual async Task<T> FindAsync(Expression<Func<T, bool>> @where)
            => await _dbSet.FirstOrDefaultAsync(where);

        public virtual T FindById(object id)
            => _dbSet.Find(id);

        public virtual async Task<T> FindByIdAsync(object id)
            => await _dbSet.FindAsync(id);

        public virtual T SingleOrDefault(Expression<Func<T, bool>> @where)
            => _dbSet.SingleOrDefault(where);

        public virtual async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> @where)
            => await _dbSet.SingleOrDefaultAsync(where);

        public virtual T SingleById(object id)
            => _dbSet.Single(p => p.Id.Equals(id));

        public virtual async Task<T> SingleByIdAsync(object id)
            => await _dbSet.SingleAsync(p => p.Id.Equals(id));

        public virtual int Count()
            => _dbSet.Count();

        public virtual int Count(Expression<Func<T, bool>> @where)
            => _dbSet.Count(@where);

        public virtual Task<int> CountAsync()
            => _dbSet.CountAsync();

        public virtual Task<int> CountAsync(Expression<Func<T, bool>> @where)
            => _dbSet.CountAsync(@where);

        public virtual bool Any()
            => _dbSet.Any();

        public virtual bool Any(Expression<Func<T, bool>> @where)
            => _dbSet.Any(where);

        public virtual Task<bool> AnyAsync()
            => _dbSet.AnyAsync();

        public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> @where)
            => _dbSet.AnyAsync(where);

        #endregion
    }
}