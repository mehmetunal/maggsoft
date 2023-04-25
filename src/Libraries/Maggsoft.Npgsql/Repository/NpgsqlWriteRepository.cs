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
    public class NpgsqlWriteRepository<T> : NpgsqlRepository<T>, INpgsqlWriteRepository<T> where T : BaseEntity, IEntity
    {
        #region Variables

        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        #endregion

        #region Constructor

        public NpgsqlWriteRepository(DbContext context)
            : base(context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        #endregion

        #region Methos

        public virtual T Add(T entity)
            => _dbSet.Add(entity).Entity;

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual void AddRange(IEnumerable<T> entities)
            => _dbSet.AddRange(entities);

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
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
        public virtual T Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
            => await Task.Run(() =>
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _dbSet.Update(entity);
                return entity;
            });

        public virtual IEnumerable<T> UpdateRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            if (!entities.Any())
                return null;

            var baseEntities = entities.ToList();
            _dbSet.UpdateRange(baseEntities);
            return baseEntities;
        }

        public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
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

        public virtual T Delete(T entity)
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

        public virtual async Task<T> DeleteAsync(T entity)
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

        public virtual void Delete(Expression<Func<T, bool>> @where)
            => Delete(_dbSet.FirstOrDefault(where));

        public virtual async Task DeleteAsync(Expression<Func<T, bool>> @where)
            => await DeleteAsync(await _dbSet.FirstOrDefaultAsync(where));

        public virtual void Delete(IEnumerable<T> entities)
            => _dbSet.RemoveRange(entities);

        public virtual async Task<IEnumerable<T>> DeleteAsync(IEnumerable<T> entities)
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

        public virtual T Delete(object id)
            => Delete(_dbSet.Find(id));

        public virtual async Task<T> DeleteAsync(object id)
            => await DeleteAsync(await _dbSet.FindAsync(id));

        #endregion
    }
}