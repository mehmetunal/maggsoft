using System;
using System.Linq;
using MongoDB.Driver;
using Maggsoft.Core.Entities;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Maggsoft.Data.Mongo;
using Maggsoft.Mongo.Extensions;
using MongoDB.Bson;

namespace Maggsoft.Mongo.Repository
{
    public sealed class MongoRepository<T> : IMongoRepository<T> where T : BaseEntity, IEntity
    {
        #region Fields

        /// <summary>
        /// Gets the collection
        /// </summary>
        public IMongoCollection<T> Collection { get; }

        /// <summary>
        /// Mongo Database
        /// </summary>
        public IMongoDatabase Database { get; }

        #endregion

        #region Ctor

        public MongoRepository(IMongoDatabase database)
        {
            Database = database;
            Collection = Database.GetCollection<T>(typeof(T).GetCollectionName());
        }

        #endregion

        #region Methos

        public IQueryable<T> Get()
            => Collection.AsQueryable();

        public async Task<IEnumerable<T>> GetAsync()
            => await Collection.Find(_ => true).ToListAsync();

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> @where)
            => Collection.Find(where).ToList();

        public IList<T> FindAll(Func<IQueryable<T>, IQueryable<T>> func = null)
        {
            IList<T> FindAll()
            {
                var query = func != null ? func(Get()) : Get();
                return query.ToList();
            }

            return FindAll();
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> @where)
            => await Collection.Find(where).ToListAsync();

        public async Task<IList<T>> FindAllAsync(Func<IQueryable<T>, IQueryable<T>> func = null)
        {
            Task<List<T>> FindAllAsync()
            {
                var query = func != null ? func(Get()) : Get();
                return Task.Run(() => query.ToList());
            }

            return await FindAllAsync();
        }

        public T Find(Expression<Func<T, bool>> @where)
            => Collection.Find(where).FirstOrDefault();

        public async Task<T> FindAsync(Expression<Func<T, bool>> @where)
            => await Collection.Find(where).FirstOrDefaultAsync();

        public T FindById(object id)
            => Collection.Find(p => p.Id == ObjectId.Parse(id.ToString())).FirstOrDefault();

        public async Task<T> FindByIdAsync(object id)
            => await Collection.Find(p => p.Id == ObjectId.Parse(id.ToString())).FirstOrDefaultAsync();

        public T SingleOrDefault(Expression<Func<T, bool>> @where)
            => Collection.Find(where).SingleOrDefault();

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> @where)
            => await Collection.Find(where).SingleOrDefaultAsync();

        public T SingleById(object id)
            => Collection.Find(p => p.Id == ObjectId.Parse(id.ToString())).Single();

        public async Task<T> SingleByIdAsync(object id)
            => await Collection.Find(p => p.Id == ObjectId.Parse(id.ToString())).SingleAsync();

        public int Count()
            => Table.Count();

        public int Count(Expression<Func<T, bool>> @where)
            => Table.Count(@where);

        public Task<int> CountAsync()
            => Table.CountAsync();

        public Task<int> CountAsync(Expression<Func<T, bool>> @where)
            => Table.CountAsync(@where);

        public bool Any()
            => Table.Any();

        public bool Any(Expression<Func<T, bool>> @where)
            => Table.Any(@where);

        public Task<bool> AnyAsync()
            => Table.AnyAsync();

        public Task<bool> AnyAsync(Expression<Func<T, bool>> @where)
            => Table.AnyAsync(@where);

        public T Add(T entity)
        {
            Collection.InsertOne(entity);
            return entity;
        }

        public async Task<T> AddAsync(T entity)
        {
            await Collection.InsertOneAsync(entity);
            return entity;
        }

        public void AddRange(IEnumerable<T> entities)
            => Collection.InsertMany(entities);

        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            var addRangeAsync = entities.ToList();
            await Collection.InsertManyAsync(addRangeAsync);
            return addRangeAsync;
        }

        public T Update(T entity)
        {
            Collection.ReplaceOne(r => r.Id == entity.Id, entity, new ReplaceOptions() { IsUpsert = false });
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            await Collection.ReplaceOneAsync(r => r.Id == entity.Id, entity, new ReplaceOptions() { IsUpsert = false });
            return entity;
        }

        public IEnumerable<T> UpdateRange(IEnumerable<T> entities)
        {
            var baseEntities = entities.ToList();
            foreach (var entity in baseEntities)
            {
                Update(entity);
            }

            return baseEntities;
        }

        public async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
        {
            var updateRangeAsync = entities.ToList();
            foreach (var entity in updateRangeAsync)
            {
                await UpdateAsync(entity);
            }

            return updateRangeAsync;
        }

        public T Delete(T entity)
            => Collection.FindOneAndDelete(d => d.Id == entity.Id);

        public async Task<T> DeleteAsync(T entity)
        {
            await Collection.DeleteOneAsync(d => d.Id == entity.Id);
            return entity;
        }

        public void Delete(Expression<Func<T, bool>> @where)
            => Collection.DeleteOne(where);

        public async Task DeleteAsync(Expression<Func<T, bool>> @where)
            => await Collection.FindOneAndDeleteAsync(where);

        public void Delete(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Collection.FindOneAndDeleteAsync(d => d.Id == entity.Id);
            }
        }

        public async Task<IEnumerable<T>> DeleteAsync(IEnumerable<T> entities)
        {
            var baseEntities = entities.ToList();
            foreach (var entity in baseEntities)
            {
                await DeleteAsync(entity);
            }

            return baseEntities;
        }

        public T Delete(object id)
            => Collection.FindOneAndDelete(d => d.Id == ObjectId.Parse(id.ToString()));

        public async Task<T> DeleteAsync(object id)
            => await Collection.FindOneAndDeleteAsync(d => d.Id == ObjectId.Parse(id.ToString()));

        #endregion

        #region Properties

        public IMongoQueryable<T> Table => Collection.AsQueryable();

        public IList<T> FindByFilterDefinition(FilterDefinition<T> query)
            => Collection.Find(query).ToList();

        #endregion
    }
}