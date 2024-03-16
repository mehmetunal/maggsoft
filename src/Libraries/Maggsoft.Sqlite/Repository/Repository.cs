using System;
using System.Linq;
using Maggsoft.Data.Sqlite;
using Maggsoft.Core.Entities;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Maggsoft.Sqlite.Repository;

public sealed class Repository<T> : IMssqlRepository<T> where T : BaseEntity, IEntity
{
    #region Variables

    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    #endregion

    #region Constructor

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    #endregion

    #region Methos

    public IQueryable<T> AsNoTrackingWithIdentityResolution()
        => _dbSet.AsNoTrackingWithIdentityResolution();

    public IQueryable<T> Get()
        => _dbSet;

    public async Task<IEnumerable<T>> GetAsync()
        => await _dbSet.ToListAsync();

    public IEnumerable<T> FindAll(Expression<Func<T, bool>> @where)
        => _dbSet.Where(where);

    public IList<T> FindAll(Func<IQueryable<T>, IQueryable<T>> func = null)
    {
        IList<T> FindAll()
        {
            var query = func != null ? func(_dbSet) : _dbSet;
            return query.ToList();
        }

        return FindAll();
    }

    public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> @where)
        => await _dbSet.Where(where).ToListAsync();

    public async Task<IList<T>> FindAllAsync(Func<IQueryable<T>, IQueryable<T>> func = null)
    {
        Task<List<T>> FindAllAsync()
        {
            var query = func != null ? func(_dbSet) : _dbSet;
            return query.ToListAsync();
        }

        return await FindAllAsync();
    }
    public T Find(Expression<Func<T, bool>> @where)
        => _dbSet.FirstOrDefault(where);

    public async Task<T> FindAsync(Expression<Func<T, bool>> @where)
        => await _dbSet.FirstOrDefaultAsync(where);

    public T FindById(object id)
        => _dbSet.Find(id);

    public async Task<T> FindByIdAsync(object id)
        => await _dbSet.FindAsync(id);

    public T SingleOrDefault(Expression<Func<T, bool>> @where)
        => _dbSet.SingleOrDefault(where);

    public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> @where)
        => await _dbSet.SingleOrDefaultAsync(where);

    public T SingleById(object id)
        => _dbSet.Single(p => p.Id.Equals(id));

    public async Task<T> SingleByIdAsync(object id)
        => await _dbSet.SingleAsync(p => p.Id.Equals(id));

    public int Count()
        => _dbSet.Count();

    public int Count(Expression<Func<T, bool>> @where)
        => _dbSet.Count(@where);

    public Task<int> CountAsync()
        => _dbSet.CountAsync();

    public Task<int> CountAsync(Expression<Func<T, bool>> @where)
        => _dbSet.CountAsync(@where);

    public bool Any()
        => _dbSet.Any();

    public bool Any(Expression<Func<T, bool>> @where)
        => _dbSet.Any(where);

    public Task<bool> AnyAsync()
        => _dbSet.AnyAsync();

    public Task<bool> AnyAsync(Expression<Func<T, bool>> @where)
        => _dbSet.AnyAsync(where);

    public T Add(T entity)
        => _dbSet.Add(entity).Entity;

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public void AddRange(IEnumerable<T> entities)
        => _dbSet.AddRange(entities);

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        if (!entities.Any())
            return null;

        var addRangeAsync = entities.ToList();
        await _dbSet.AddRangeAsync(addRangeAsync);
        return addRangeAsync;
    }
    /// <summary>
    ///     if (order != null)
    //    {
    //        await _context.Entry(order)
    //            .Collection(i => i.OrderItems).LoadAsync();
    //    await _context.Entry(order)
    //            .Reference(i => i.OrderStatus).LoadAsync();
    //}

    /*
     https://stackoverflow.com/questions/43571338/ef-core-helper-method-for-explicit-loading-references-and-collections
     public async Task Load<TEntity>(TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions)
        where TEntity : class
    {
        foreach (var propertyExpression in propertyExpressions)
        {
            var propertyName = propertyExpression.GetPropertyAccess().Name;
            await Entry(entity).Navigation(propertyName).LoadAsync();
        }
    }
        public async Task Load<TEntity>(TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions)
          where TEntity : class
        {
          foreach (var propertyExpression in propertyExpressions) {
            var isCollection = typeof(IEnumerable).GetTypeInfo()
                               .IsAssignableFrom(propertyExpression.Body.Type);
            if(isCollection)
            {
              await Entry(entity)
                .Collection(propertyExpression)     // problem is here !!!!!
                .LoadAsync();
            }
            else
            {
              await Entry(entity)
                .Reference(propertyExpression)
                .LoadAsync();
            }
          }
        }
     */
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public T Update(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
        => await Task.Run(() =>
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _dbSet.Update(entity);
            return entity;
        });

    public IEnumerable<T> UpdateRange(IEnumerable<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        if (!entities.Any())
            return null;

        var baseEntities = entities.ToList();
        _dbSet.UpdateRange(baseEntities);
        return baseEntities;
    }

    public async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
        => await Task.Run(() =>
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            if (!entities.Any())
                return null;
            var baseEntities = entities.ToList();
            _dbSet.UpdateRange(baseEntities);
            return baseEntities;
        });

    public T Delete(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _context.Attach(entity);
        }

        var remove = _dbSet.Remove(entity);
        return remove.Entity;
    }

    public async Task<T> DeleteAsync(T entity)
        => await Task.Run(() =>
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Attach(entity);
            }
            var remove = _dbSet.Remove(entity);
            return remove.Entity;
        });

    public void Delete(Expression<Func<T, bool>> @where)
        => Delete(Find(where));

    public async Task DeleteAsync(Expression<Func<T, bool>> @where)
        => await DeleteAsync(await FindAsync(where));

    public void Delete(IEnumerable<T> entities)
        => _dbSet.RemoveRange(entities);

    public async Task<IEnumerable<T>> DeleteAsync(IEnumerable<T> entities)
        => await Task.Run(() =>
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            if (!entities.Any())
                return null;
            var baseEntities = entities.ToList();
            _dbSet.RemoveRange(baseEntities);
            return baseEntities;
        });

    public T Delete(object id)
        => Delete(FindById(id));

    public async Task<T> DeleteAsync(object id)
        => await DeleteAsync(await FindByIdAsync(id));

    #endregion

    #region SaveChange
    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    #endregion

    #region CustomMethod

    public IQueryable<T> FromSqlRaw(string sql, params object[] par)
        => _dbSet.FromSqlRaw(sql, par);

    public int Execute(string sql, params object[] par)
        => _context.Database.ExecuteSqlRaw(sql, par);

    public async Task<int> ExecuteAsync(string sql, params object[] par)
        => await _context.Database.ExecuteSqlRawAsync(sql, par);

    public IQueryable<T> Table => _dbSet.AsQueryable();

    #endregion
}