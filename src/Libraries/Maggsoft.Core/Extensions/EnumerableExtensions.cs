using AutoMapper.QueryableExtensions;
using Maggsoft.Core.Mapper;
using Maggsoft.Core.Model;
using Maggsoft.Core.Model.DataTables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
        foreach (var f in args.Where(w => !string.IsNullOrEmpty(w.Field) && w.Value.IsNotNull()))
        {
            var propertyInfo = typeof(TSource).GetProperty(f.Field.Trim(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propertyInfo == null)
                continue;
            var propertyExpression = Expression.Property(parameter, propertyInfo.Name);
            var propType = propertyInfo.PropertyType;
            var isNullableDateTime = propType == typeof(DateTime?) || (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>) && propType.GetGenericArguments()[0] == typeof(DateTime));
            var isDateTime = propType == typeof(DateTime) || isNullableDateTime;

            var propertyOperation = propertyInfo.GetCustomAttributes<DTFilterOperation>(true).FirstOrDefault()?.Name;
            if (string.IsNullOrEmpty(propertyOperation) && isDateTime)
                f.Operator = "eq";
            if (!string.IsNullOrEmpty(propertyOperation))
                f.Operator = propertyOperation;

            object propertyValue;
            var raw = f.Operator == Operators.Contains ? f.Value?.ToString()?.ToLower() : f.Value?.ToString();
            if (string.IsNullOrEmpty(raw))
                continue;

            if (isDateTime)
            {
                if (!DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
                    continue;
                propertyValue = isNullableDateTime ? (DateTime?)dt : dt;
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>)
                    ? Nullable.GetUnderlyingType(propType) ?? propType
                    : propType);
                try
                {
                    propertyValue = converter.ConvertFromInvariantString(raw)!;
                }
                catch
                {
                    try
                    {
                        propertyValue = converter.ConvertFrom(null, CultureInfo.InvariantCulture, raw)!;
                    }
                    catch
                    {
                        continue;
                    }
                }
                if (propertyValue == null)
                    continue;
            }

            // Ensure propertyValue matches the property type exactly to avoid SQL conversion issues
            // EF Core needs the constant to match the property type for proper parameter generation
            object typedValue = propertyValue;
            if (propertyValue.GetType() != propertyInfo.PropertyType)
            {
                if (propertyInfo.PropertyType == typeof(DateTime?) && propertyValue is DateTime dt)
                {
                    typedValue = (DateTime?)dt;
                }
                else if (propertyInfo.PropertyType == typeof(DateTime) && propertyValue is DateTime dtValue)
                {
                    typedValue = dtValue;
                }
                else if (propertyInfo.PropertyType == typeof(DateTime) && propertyValue is DateTime?)
                {
                    var dtNullable = (DateTime?)propertyValue;
                    typedValue = dtNullable.Value;
                }
                else if (!propertyInfo.PropertyType.IsInstanceOfType(propertyValue))
                {
                    // Try to convert if types don't match
                    try
                    {
                        typedValue = Convert.ChangeType(propertyValue, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                        if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            typedValue = Activator.CreateInstance(propertyInfo.PropertyType, typedValue)!;
                        }
                    }
                    catch
                    {
                        continue; // Skip this filter if conversion fails
                    }
                }
            }

            // Use closure pattern to force EF Core to use SQL parameters instead of string literals
            // This is critical for DateTime values to avoid SQL conversion errors
            // Lambda closure approach: create a lambda that captures the value, forcing parameterization
            Expression valueExpression;
            if (isDateTime)
            {
                // For DateTime, use lambda closure to ensure EF Core parameterizes the value
                // This prevents SQL conversion errors by using parameters instead of string literals
                var closureValue = typedValue;
                if (propertyInfo.PropertyType == typeof(DateTime))
                {
                    Expression<Func<DateTime>> valueLambda = () => (DateTime)closureValue;
                    valueExpression = valueLambda.Body;
                }
                else // DateTime?
                {
                    Expression<Func<DateTime?>> valueLambda = () => (DateTime?)closureValue;
                    valueExpression = valueLambda.Body;
                }
            }
            else
            {
                // For other types, use constant directly
                valueExpression = Expression.Constant(typedValue, propertyInfo.PropertyType);
            }
            Expression filter;
            
            // For DateTime fields with 'eq' operator, check if this is a Date-only filter
            // If the time component is 00:00:00, treat it as a date range filter (start of day to start of next day)
            if (isDateTime && f.Operator == Operators.Equal)
            {
                DateTime? filterDateNullable = null;
                if (typedValue is DateTime dt)
                    filterDateNullable = dt;
                else if (typedValue != null && typedValue.GetType() == typeof(DateTime?))
                {
                    var dtNullable = (DateTime?)typedValue;
                    if (dtNullable.HasValue)
                        filterDateNullable = dtNullable.Value;
                }
                
                if (!filterDateNullable.HasValue)
                {
                    filter = Expression.Equal(propertyExpression, valueExpression);
                }
                else
                {
                    var filterDate = filterDateNullable.Value;
                    
                    // Check if this is a date-only filter (time is 00:00:00 or very close to midnight)
                    // This handles DatePicker filters where only date is selected
                    // Also check if time is exactly midnight (UTC conversion might shift it slightly)
                    if (filterDate.TimeOfDay.TotalSeconds < 1 || 
                        (filterDate.Hour == 0 && filterDate.Minute == 0 && filterDate.Second == 0) ||
                        (filterDate.Hour >= 21 && filterDate.Hour <= 23)) // UTC conversion: TR timezone is UTC+3, so 00:00 TR = 21:00 UTC previous day
                    {
                        // Convert to date range: >= startOfDay AND < startOfNextDay
                        // Handle UTC conversion: if time is 21:00-23:59 UTC, it's likely the previous day in local time
                        DateTime startOfDay;
                        if (filterDate.Hour >= 21 && filterDate.Hour <= 23)
                        {
                            // UTC time represents previous day in local timezone (UTC+3)
                            startOfDay = filterDate.Date.AddDays(1); // Next day in local time
                        }
                        else
                        {
                            startOfDay = filterDate.Date; // 00:00:00 of the selected date
                        }
                        var startOfNextDay = startOfDay.AddDays(1); // 00:00:00 of next day
                        
                        // Create expressions for startOfDay and startOfNextDay
                        Expression startExpression, endExpression;
                        if (propertyInfo.PropertyType == typeof(DateTime))
                        {
                            Expression<Func<DateTime>> startLambda = () => startOfDay;
                            Expression<Func<DateTime>> endLambda = () => startOfNextDay;
                            startExpression = startLambda.Body;
                            endExpression = endLambda.Body;
                        }
                        else // DateTime?
                        {
                            Expression<Func<DateTime?>> startLambda = () => (DateTime?)startOfDay;
                            Expression<Func<DateTime?>> endLambda = () => (DateTime?)startOfNextDay;
                            startExpression = startLambda.Body;
                            endExpression = endLambda.Body;
                        }
                        
                        // Create: property >= startOfDay AND property < startOfNextDay
                        var greaterThanOrEqual = Expression.GreaterThanOrEqual(propertyExpression, startExpression);
                        var lessThan = Expression.LessThan(propertyExpression, endExpression);
                        filter = Expression.AndAlso(greaterThanOrEqual, lessThan);
                    }
                    else
                    {
                        // Regular DateTime equality (with time component)
                        filter = Expression.Equal(propertyExpression, valueExpression);
                    }
                }
            }
            else
            {
                // For non-DateTime fields or non-eq operators, use standard equality
                filter = Expression.Equal(propertyExpression, valueExpression);
            }

            if (f.Operator == Operators.NotEqual || f.Operator == Operators.IsNotNull)
                filter = Expression.NotEqual(propertyExpression, valueExpression);
            else if (f.Operator == Operators.StartsWith)
                filter = Expression.Call(propertyExpression, typeof(string).GetMethod("StartsWith", [typeof(string)])!, valueExpression);
            else if (f.Operator == Operators.Contains)
            {
                var toLower = Expression.Call(propertyExpression, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes)!);
                filter = Expression.Call(toLower, typeof(string).GetMethod("Contains", [typeof(string)])!, valueExpression);
            }
            //filter = Expression.Call(member, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), valueExpression);
            else if (f.Operator == Operators.EndsWith)
                filter = Expression.Call(propertyExpression, typeof(string).GetMethod("EndsWith", [typeof(string)])!, valueExpression);
            else if (f.Operator == Operators.DoesNotContain)
                filter = Expression.Not(Expression.Call(propertyExpression, typeof(string).GetMethod("Contains", [typeof(string)
                ])!, valueExpression));
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
