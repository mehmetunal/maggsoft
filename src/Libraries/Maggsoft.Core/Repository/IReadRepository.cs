using Maggsoft.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Maggsoft.Core.Repository
{
    public interface IReadRepository<T> : IRepository<T> where T : IEntity
    {
        #region GET

        IQueryable<T> Get();
        Task<IEnumerable<T>> GetAsync();

        IEnumerable<T> FindAll(Expression<Func<T, bool>> where);
        IList<T> FindAll(Func<IQueryable<T>, IQueryable<T>> func = null);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> where);
        Task<IList<T>> FindAllAsync(Func<IQueryable<T>, IQueryable<T>> func = null);

        T Find(Expression<Func<T, bool>> where);
        Task<T> FindAsync(Expression<Func<T, bool>> where);
        T FindById(object id);
        Task<T> FindByIdAsync(object id);


        T SingleOrDefault(Expression<Func<T, bool>> @where);
        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> @where);
        T SingleById(object id);
        Task<T> SingleByIdAsync(object id);

        #endregion

        #region Count
        int Count();
        int Count(Expression<Func<T, bool>> @where);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> @where);
        #endregion

        #region Any
        bool Any();
        bool Any(Expression<Func<T, bool>> @where);
        Task<bool> AnyAsync();
        Task<bool> AnyAsync(Expression<Func<T, bool>> @where);
        #endregion
    }
}
