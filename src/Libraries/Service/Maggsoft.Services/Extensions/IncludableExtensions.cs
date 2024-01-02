using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Maggsoft.Services.Extensions
{
    public interface IIncludable { }

    public interface IIncludable<out TEntity> : IIncludable { }

    public interface IIncludable<out TEntity, out TProperty> : IIncludable<TEntity> { }

    internal class Includable<TEntity> : IIncludable<TEntity> where TEntity : class
    {
        internal IQueryable<TEntity> Input { get; }

        internal Includable(IQueryable<TEntity> queryable)
        {
            // C# 7 syntax, just rewrite it "old style" if you do not have Visual Studio 2017
            Input = queryable ?? throw new ArgumentNullException(nameof(queryable));
        }
    }

    internal class Includable<TEntity, TProperty> :
        Includable<TEntity>, IIncludable<TEntity, TProperty>
        where TEntity : class
    {
        internal IIncludableQueryable<TEntity, TProperty> IncludableInput { get; }

        internal Includable(IIncludableQueryable<TEntity, TProperty> queryable) :
            base(queryable)
        {
            IncludableInput = queryable;
        }
    }

    public static class IncludableExtensions
    {
        public static IIncludable<TEntity, TProperty> Include<TEntity, TProperty>(this IIncludable<TEntity> includes, Expression<Func<TEntity, TProperty>> propertySelector) where TEntity : class
        {
            var result = ((Includable<TEntity>)includes).Input.Include(propertySelector);
            return new Includable<TEntity, TProperty>(result);
        }

        public static IIncludable<TEntity, TOtherProperty> ThenInclude<TEntity, TOtherProperty, TProperty>(this IIncludable<TEntity, TProperty> includes, Expression<Func<TProperty, TOtherProperty>> propertySelector) where TEntity : class
        {
            var result = ((Includable<TEntity, TProperty>)includes).IncludableInput.ThenInclude(propertySelector);
            return new Includable<TEntity, TOtherProperty>(result);
        }

        public static IIncludable<TEntity, TOtherProperty> ThenInclude<TEntity, TOtherProperty, TProperty>(this IIncludable<TEntity, IEnumerable<TProperty>> includes, Expression<Func<TProperty, TOtherProperty>> propertySelector) where TEntity : class
        {
            var result = ((Includable<TEntity, IEnumerable<TProperty>>)includes).IncludableInput.ThenInclude(propertySelector);
            return new Includable<TEntity, TOtherProperty>(result);
        }

        public static IIncludable<TEntity, TOtherProperty> ThenInclude<TEntity, TOtherProperty, TProperty>(this IIncludable<TEntity, IOrderedEnumerable<TProperty>> includes, Expression<Func<TProperty, TOtherProperty>> propertySelector) where TEntity : class
        {
            var result = ((Includable<TEntity, IOrderedEnumerable<TProperty>>)includes).IncludableInput.ThenInclude(propertySelector);
            return new Includable<TEntity, TOtherProperty>(result);
        }

        public static IIncludable<TEntity, TOtherProperty> ThenInclude<TEntity, TOtherProperty, TProperty>(this IIncludable<TEntity, ICollection<TProperty>> includes, Expression<Func<TProperty, TOtherProperty>> propertySelector) where TEntity : class
        {
            var result = ((Includable<TEntity, ICollection<TProperty>>)includes).IncludableInput.ThenInclude(propertySelector);
            return new Includable<TEntity, TOtherProperty>(result);
        }

        public static IQueryable<T> IncludeMultiple<T>(this IQueryable<T> query, Func<IIncludable<T>, IIncludable> includes) where T : class
        {
            if (includes == null)
                return query;

            var includable = (Includable<T>)includes(new Includable<T>(query));
            return includable.Input;
        }

        public static IQueryable<T> IncludeMultiple<T>(this IQueryable<T> query, params Expression<Func<T, object>>[] includes) where T : class
        {
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return query;
        }
    }
}