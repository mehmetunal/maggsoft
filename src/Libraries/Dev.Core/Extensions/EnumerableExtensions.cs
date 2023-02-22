using Dev.Core.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Dev.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static TSource NextOrDefault<TSource>(this IEnumerable<TSource> source, TSource indexData)
           => source.SkipWhile(x => !x.Equals(indexData)).Skip(1).FirstOrDefault();

        public static TSource PreviousOrDefault<TSource>(this IEnumerable<TSource> source, TSource indexData)
            => source.TakeWhile(x => !x.Equals(indexData)).LastOrDefault();

        public static IQueryable<TSource> AddFilterQuery<TSource>(this IQueryable<TSource> q, List<Filter> args)
        {
            if (args.HasNotFilter())
                return q;

            var parameter = Expression.Parameter(typeof(TSource), "x");
            foreach (var f in args.Where(w => !string.IsNullOrEmpty(w.Field) && w.Value.IsNotNull()))
            {
                var prop = typeof(TSource).GetProperty(f.Field);
                var member = Expression.Property(parameter, prop.Name);
                var converter = TypeDescriptor.GetConverter(prop.PropertyType); // 1
                object propertyValue = null;
                try
                {
                    propertyValue = converter.ConvertFromInvariantString(f.Value.ToString()); // 3
                }
                catch
                {
                    propertyValue = converter.ConvertFrom(f.Value.ToString()); // 3
                }

                var constant = Expression.Constant(propertyValue);
                var valueExpression = Expression.Convert(constant, prop.PropertyType); // 4
                Expression filter = Expression.Equal(member, valueExpression);

                if (f.Operator == Operators.NotEqual || f.Operator == Operators.IsNotNull)
                    filter = Expression.NotEqual(member, valueExpression);
                else if (f.Operator == Operators.StartsWith)
                    filter = Expression.Call(member, typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }), valueExpression);
                else if (f.Operator == Operators.Contains)
                    filter = Expression.Call(member, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), valueExpression);
                else if (f.Operator == Operators.EndsWith)
                    filter = Expression.Call(member, typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) }), valueExpression);
                else if (f.Operator == Operators.DoesNotContain)
                    filter = Expression.Not(Expression.Call(member, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), valueExpression));
                else if (f.Operator == Operators.GreaterThan)
                    filter = Expression.GreaterThan(member, valueExpression);
                else if (f.Operator == Operators.GreaterThanOrEqual)
                    filter = Expression.GreaterThanOrEqual(member, valueExpression);
                else if (f.Operator == Operators.LessThan)
                    filter = Expression.LessThan(member, valueExpression);
                else if (f.Operator == Operators.LessThanOrEqual)
                    filter = Expression.LessThanOrEqual(member, valueExpression);

                var lambda = Expression.Lambda<Func<TSource, bool>>(filter, parameter);
                q = q.Where(lambda);
            }

            return q;
        }

        public static IQueryable<T> AddSortQuery<T>(this IQueryable<T> q, List<Sort> args)
        {
            if (args.HasNotSort())
                return q;

            foreach (var sort in args)
            {
                var classPara = Expression.Parameter(typeof(T), "t");
                var pi = typeof(T).GetProperty(sort.Field);
                q = q.Provider.CreateQuery<T>(Expression.Call(typeof(Queryable), sort.Asc ? "OrderBy" : "OrderByDescending", new Type[] { typeof(T), pi.PropertyType }, q.Expression, Expression.Lambda(Expression.Property(classPara, pi), classPara)));
            }

            return q;
        }

        public static bool IsEmpty<TSource>(this IEnumerable<TSource> source)
            => source == null || source.Any();

        public static bool IsEmpty(this object source)
            => source == null;
    }
}
