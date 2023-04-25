using Maggsoft.Core.Entities;
using Maggsoft.Data.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Maggsoft.Mongo.Repository
{
    public class MongoWriteRepository<T> : MongoRepository<T>, IMongoWriteRepository<T> where T : BaseEntity, IEntity
    {
        #region Fields

        #endregion

        #region Ctor

        public MongoWriteRepository(IMongoDatabase database) 
            : base(database)
        {
        }

        #endregion

        #region Methos

        public virtual T Add(T entity)
        {
            Collection.InsertOne(entity);
            return entity;
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await Collection.InsertOneAsync(entity);
            return entity;
        }

        public virtual void AddRange(IEnumerable<T> entities)
            => Collection.InsertMany(entities);

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            var addRangeAsync = entities.ToList();
            await Collection.InsertManyAsync(addRangeAsync);
            return addRangeAsync;
        }

        public virtual T Update(T entity)
        {
            Collection.ReplaceOne(r => r.Id == entity.Id, entity, new ReplaceOptions() { IsUpsert = false });
            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            await Collection.ReplaceOneAsync(r => r.Id == entity.Id, entity, new ReplaceOptions() { IsUpsert = false });
            return entity;
        }

        public virtual IEnumerable<T> UpdateRange(IEnumerable<T> entities)
        {
            var baseEntities = entities.ToList();
            foreach (var entity in baseEntities)
            {
                Update(entity);
            }

            return baseEntities;
        }

        public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
        {
            var updateRangeAsync = entities.ToList();
            foreach (var entity in updateRangeAsync)
            {
                await UpdateAsync(entity);
            }

            return updateRangeAsync;
        }

        public virtual T Delete(T entity)
            => Collection.FindOneAndDelete(d => d.Id == entity.Id);

        public virtual async Task<T> DeleteAsync(T entity)
        {
            await Collection.DeleteOneAsync(d => d.Id == entity.Id);
            return entity;
        }

        public virtual void Delete(Expression<Func<T, bool>> @where)
            => Collection.DeleteOne(where);

        public virtual async Task DeleteAsync(Expression<Func<T, bool>> @where)
            => await Collection.FindOneAndDeleteAsync(where);

        public virtual void Delete(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Collection.FindOneAndDeleteAsync(d => d.Id == entity.Id);
            }
        }

        public virtual async Task<IEnumerable<T>> DeleteAsync(IEnumerable<T> entities)
        {
            var baseEntities = entities.ToList();
            foreach (var entity in baseEntities)
            {
                await DeleteAsync(entity);
            }

            return baseEntities;
        }

        public virtual T Delete(object id)
            => Collection.FindOneAndDelete(d => d.Id == ObjectId.Parse(id.ToString()));

        public virtual async Task<T> DeleteAsync(object id)
            => await Collection.FindOneAndDeleteAsync(d => d.Id == ObjectId.Parse(id.ToString()));

        #endregion

        #region Properties

        #endregion
    }
}