using System;
using System.Linq;
using Maggsoft.Core.Entities;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Maggsoft.Core.Repository
{
    //@TODO: Repository Write,Read Repository olarak parçalanması gerekecek.
    public interface IRepository<T> where T : IEntity
    {
        #region READ

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

        #endregion

        #region WRITE

        #region ADD

        T Add(T entity);
        Task<T> AddAsync(T entity);
        void AddRange(IEnumerable<T> entities);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        #endregion

        #region UPDATE

        T Update(T entity);
        Task<T> UpdateAsync(T entity);
        IEnumerable<T> UpdateRange(IEnumerable<T> entities);
        Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities);

        #endregion

        #region DELETE

        T Delete(T entity);
        Task<T> DeleteAsync(T entity);

        void Delete(Expression<Func<T, bool>> where);
        Task DeleteAsync(Expression<Func<T, bool>> where);

        void Delete(IEnumerable<T> entities);
        Task<IEnumerable<T>> DeleteAsync(IEnumerable<T> entities);

        T Delete(object id);
        Task<T> DeleteAsync(object id);

        #endregion

        #endregion

    }
}
