using Maggsoft.Core.Entities;
using Maggsoft.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maggsoft.Core.Extensions;

public static class GlobalExtensions
{
    public static bool IsDefault<T>(this T obj) where T : struct, IEquatable<T>
        => EqualityComparer<T>.Default.Equals(obj, default(T));

    public static IEnumerable<string> AlphaLengthWise(this string[] data)
    {
        if (data == null)
            throw new ArgumentNullException("names");

        return data.OrderBy(a => a.Length).ThenBy(a => a);
    }

    public static IEnumerable<T> AlphaLengthWise<T, L>(this IEnumerable<T> data, Func<T, L> lengthProvider)
    {
        if (data == null)
            throw new ArgumentNullException("data");

        if (lengthProvider == null)
            throw new ArgumentNullException("lengthProvider");

        return data.OrderBy(a => lengthProvider(a)).ThenBy(a => a);
    }

    public static IOrderedQueryable<string> AlphaLengthWise(this IQueryable<string> data)
    {
        if (data == null)
            throw new ArgumentNullException("data");
        return data.OrderBy(a => a.Length).ThenBy(a => a);
    }

    public static IOrderedEnumerable<string> AlphaLengthWise(this IEnumerable<string> data)
    {
        if (data == null)
            throw new ArgumentNullException("data");

        return data.OrderBy(a => a.Length).ThenBy(a => a);
    }

    public static TEntity ConstRun<TEntity>(this IEntity model) where TEntity : IEntity, new()
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));
        var item = new TEntity();
        return item;
    }

    public static bool IsNotNull(this object val)
        => val != null && val != DBNull.Value;

    public static bool HasFilter(this List<Filter> args)
        => args.IsNotNull() && args.Count.IsGreaterThanZero() ? true : false;

    public static bool HasNotFilter(this List<Filter> args)
        => args.HasFilter() ? false : true;

    public static bool IsGreaterThanZero(this int val)
        => val > 0 ? true : false;

    public static bool HasSort(this List<Sort> args)
        => args.IsNotNull() && args.Count.IsGreaterThanZero() ? true : false;

    public static bool HasNotSort(this List<Sort> args)
        => args.HasSort() ? false : true;

    public static T Clone<T>(this T listToClone) where T : ICloneable
        => (T)listToClone.Clone();

    public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        => listToClone.Select(item => (T)item.Clone()).ToList();
}
