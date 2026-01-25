using AutoMapper.QueryableExtensions;
using Maggsoft.Core.Mapper;
using Maggsoft.Core.Model;
using Maggsoft.Core.Model.DataTables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Maggsoft.Core.Extensions;

public static class EnumerableExtensions
{
    public static TSource NextOrDefault<TSource>(this IEnumerable<TSource> source, TSource indexData)
       => source.SkipWhile(x => !x.Equals(indexData)).Skip(1).FirstOrDefault();

    public static TSource PreviousOrDefault<TSource>(this IEnumerable<TSource> source, TSource indexData)
        => source.TakeWhile(x => !x.Equals(indexData)).LastOrDefault();

    public static IEnumerable<TSource> ToBetween<TSource>(this IEnumerable<TSource> source, dynamic s, dynamic e)
    {
        bool f = true;
        var result = source.SkipWhile(p => p != s).TakeWhile(p => { bool b = true; f = p != e; return b; });
        return result;
    }

    public static IEnumerable<TSource> ToBetween<TSource>(this IEnumerable<TSource> source, TSource s, TSource e)
    {
        bool f = true;
        var result = source.SkipWhile(x => !x.Equals(s)).TakeWhile(p => { bool b = true; f = !p.Equals(e); return b; });
        return result;
    }

    public static IEnumerable<string> ToBetween(this List<string> sender, string startValue, string endValue)
    {
        var startIndex = sender.IndexOf(startValue);
        var endIndex = sender.IndexOf(endValue) - startIndex + 1;

        return startIndex == -1 || endIndex == -1 ? null : sender.GetRange(startIndex, endIndex);
    }

    public static IEnumerable<object> ToBetween(this List<object> sender, object startValue, object endValue)
    {
        var startIndex = sender.IndexOf(startValue);
        var endIndex = sender.IndexOf(endValue) - startIndex + 1;

        return startIndex == -1 || endIndex == -1 ? null : sender.GetRange(startIndex, endIndex);
    }

    public static IEnumerable<T> ToBetween<T>(this List<T> sender, T startValue, T endValue)
    {
        var startIndex = sender.IndexOf(startValue);
        var endIndex = sender.IndexOf(endValue) - startIndex + 1;

        return startIndex == -1 || endIndex == -1 ? null : sender.GetRange(startIndex, endIndex);
    }

    /// <summary>
    /// https://medium.com/@maghawry.hussein20/how-to-generate-ef-queries-dynamically-75e0343c536a
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="q"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IQueryable<TSource> AddFilterQuery<TSource>(this IQueryable<TSource> q, List<Filter> args)
    {
        if (args.HasNotFilter())
            return q;

        var parameter = Expression.Parameter(typeof(TSource), "x");
        foreach (var f in args.Where(w => w != null && !string.IsNullOrEmpty(w.Field) && w.Value.IsNotNull()))
        {
            var propertyInfo = typeof(TSource).GetProperty(f.Field.Trim(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propertyInfo == null)
                continue;
            var propertyExpression = Expression.Property(parameter, propertyInfo.Name);
            var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType); // 1
            var propertyOperation = propertyInfo.GetCustomAttributes<DTFilterOperation>(true).FirstOrDefault()?.Name;
            object propertyValue = null;

            if (string.IsNullOrEmpty(propertyOperation) && (converter is DateTimeConverter || f.Operator == "contains" && (propertyInfo.PropertyType != typeof(string) && propertyInfo.PropertyType != typeof(String))))
                f.Operator = "eq";
            if (!string.IsNullOrEmpty(propertyOperation))
                f.Operator = propertyOperation;

            try
            {
                propertyValue = converter.ConvertFromInvariantString(
                    f.Operator == Operators.Contains
                    ? f.Value.ToString().ToLower()
                    : f.Value.ToString()
                    ); // 3
            }
            catch
            {
                propertyValue = converter.ConvertFrom(
                    f.Operator == Operators.Contains
                    ? f.Value.ToString().ToLower()
                    : f.Value.ToString()
                    ); // 3
            }
            var constant = Expression.Constant(propertyValue);
            var valueExpression = Expression.Convert(constant, propertyInfo.PropertyType); // 4
            Expression filter = Expression.Equal(propertyExpression, valueExpression);

            if (f.Operator == Operators.NotEqual || f.Operator == Operators.IsNotNull)
                filter = Expression.NotEqual(propertyExpression, valueExpression);
            else if (f.Operator == Operators.StartsWith)
                filter = Expression.Call(propertyExpression, typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }), valueExpression);
            else if (f.Operator == Operators.Contains)
            {
                var toLower = Expression.Call(propertyExpression, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
                filter = Expression.Call(toLower, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), valueExpression);
            }
            //filter = Expression.Call(member, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), valueExpression);
            else if (f.Operator == Operators.EndsWith)
                filter = Expression.Call(propertyExpression, typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) }), valueExpression);
            else if (f.Operator == Operators.DoesNotContain)
                filter = Expression.Not(Expression.Call(propertyExpression, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), valueExpression));
            else if (f.Operator == Operators.GreaterThan)
                filter = Expression.GreaterThan(propertyExpression, valueExpression);
            else if (f.Operator == Operators.GreaterThanOrEqual)
                filter = Expression.GreaterThanOrEqual(propertyExpression, valueExpression);
            else if (f.Operator == Operators.LessThan)
                filter = Expression.LessThan(propertyExpression, valueExpression);
            else if (f.Operator == Operators.LessThanOrEqual)
                filter = Expression.LessThanOrEqual(propertyExpression, valueExpression);

            string[] sourceArray = ["startswith", "contains", "endswith", "doesnotcontain"];
            if (sourceArray.Contains(f.Operator))
            {
                var notNullExpression = Expression.NotEqual(propertyExpression, Expression.Constant(null, propertyInfo.PropertyType));
                filter = Expression.AndAlso(notNullExpression, filter);
                /*
                    *** supporting the OR logical operator ***
                    
                    if (statement.Conector == FilterStatementConector.And)
                    {
                        finalExpression = Expression.AndAlso(finalExpression, expression);
                    }
                    else
                    {
                        finalExpression = Expression.OrElse(finalExpression, expression);
                    }

                 */
            }

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
            if (sort == null || string.IsNullOrWhiteSpace(sort.Field)) continue;
            var pi = typeof(T).GetProperty(sort.Field.Trim(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pi == null) continue;
            var classPara = Expression.Parameter(typeof(T), "t");
            q = q.Provider.CreateQuery<T>(Expression.Call(typeof(Queryable), sort.Asc ? "OrderBy" : "OrderByDescending", new Type[] { typeof(T), pi.PropertyType }, q.Expression, Expression.Lambda(Expression.Property(classPara, pi), classPara)));
        }

        return q;
    }

    public static bool IsEmpty<TSource>(this IEnumerable<TSource> source)
              => source == null || source.Any();

    public static bool IsEmpty(this object source)
        => source == null;

    public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source)
    {
        return source.ProjectTo<TDestination>(AutoMapperConfiguration.MapperConfiguration);
    }
}
