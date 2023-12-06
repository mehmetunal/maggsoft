using System;
using System.Linq;
using System.Linq.Expressions;

namespace Maggsoft.Core.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> queryable,
            bool condition,
            Expression<Func<T,bool>> predicate)
        {
            if (condition)
            {
                queryable = queryable.Where(predicate);
            }

            return queryable;
        }
    }
}
