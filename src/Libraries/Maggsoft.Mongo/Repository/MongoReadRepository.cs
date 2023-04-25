using Maggsoft.Core.Entities;
using Maggsoft.Data.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Maggsoft.Mongo.Repository
{
    public class MongoReadRepository<T> : MongoRepository<T>, IMongoReadRepository<T> where T : BaseEntity, IEntity
    {
        #region Fields

        #endregion
        
        #region Ctor

        public MongoReadRepository(IMongoDatabase database) 
            : base(database)
        {
        }

        #endregion

        #region Methos

        public virtual IQueryable<T> Get()
            => Collection.AsQueryable();

        public virtual async Task<IEnumerable<T>> GetAsync()
            => await Collection.Find(_ => true).ToListAsync();

        public virtual IEnumerable<T> FindAll(Expression<Func<T, bool>> @where)
            => Collection.Find(where).ToList();

        public virtual IList<T> FindAll(Func<IQueryable<T>, IQueryable<T>> func = null)
        {
            IList<T> FindAll()
            {
                var query = func != null ? func(Get()) : Get();
                return query.ToList();
            }

            return FindAll();
        }

        public virtual async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> @where)
            => await Collection.Find(where).ToListAsync();

        public virtual async Task<IList<T>> FindAllAsync(Func<IQueryable<T>, IQueryable<T>> func = null)
        {
            Task<List<T>> FindAllAsync()
            {
                var query = func != null ? func(Get()) : Get();
                return Task.Run(() => query.ToList());
            }

            return await FindAllAsync();
        }

        public virtual T Find(Expression<Func<T, bool>> @where)
            => Collection.Find(where).FirstOrDefault();

        public virtual async Task<T> FindAsync(Expression<Func<T, bool>> @where)
            => await Collection.Find(where).FirstOrDefaultAsync();

        public virtual T FindById(object id)
            => Collection.Find(p => p.Id == ObjectId.Parse(id.ToString())).FirstOrDefault();

        public virtual async Task<T> FindByIdAsync(object id)
            => await Collection.Find(p => p.Id == ObjectId.Parse(id.ToString())).FirstOrDefaultAsync();

        public T SingleOrDefault(Expression<Func<T, bool>> @where)
            => Collection.Find(where).SingleOrDefault();

        public virtual async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> @where)
            => await Collection.Find(where).SingleOrDefaultAsync();

        public virtual T SingleById(object id)
            => Collection.Find(p => p.Id == ObjectId.Parse(id.ToString())).Single();

        public virtual async Task<T> SingleByIdAsync(object id)
            => await Collection.Find(p => p.Id == ObjectId.Parse(id.ToString())).SingleAsync();

        public virtual int Count()
            => Table.Count();

        public virtual int Count(Expression<Func<T, bool>> @where)
            => Table.Count(@where);

        public virtual Task<int> CountAsync()
            => Table.CountAsync();

        public virtual Task<int> CountAsync(Expression<Func<T, bool>> @where)
            => Table.CountAsync(@where);

        public virtual bool Any()
            => Table.Any();

        public virtual bool Any(Expression<Func<T, bool>> @where)
            => Table.Any(@where);

        public virtual Task<bool> AnyAsync()
            => Table.AnyAsync();

        public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> @where)
            => Table.AnyAsync(@where);
        #endregion

        #region Properties

        #endregion
    }
}