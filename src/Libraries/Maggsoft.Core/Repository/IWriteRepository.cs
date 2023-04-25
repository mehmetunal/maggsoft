using Maggsoft.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Maggsoft.Core.Repository
{
    public interface IWriteRepository<T> : IRepository<T> where T : IEntity
    {
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
    }
}
