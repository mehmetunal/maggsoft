using System.Collections.Generic;
using Maggsoft.Core.Model.Pagination;
using System.Linq;
using System.Threading.Tasks;
using Maggsoft.Core.Model;

namespace Maggsoft.Core.Extensions;

public static class PaginationExtension
{
    public static Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize, List<Filter> filters = null, List<Sort> sorts = null)
        => Task.FromResult(ToPagedList(query, new PaginationFilter(pageNumber, pageSize, filters, sorts)));

    public static Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, PaginationFilter paginationFilter)
        => Task.FromResult(ToPagedList(query, paginationFilter));

    public static PagedList<T> ToPagedList<T>(this IQueryable<T> query, int pageNumber, int pageSize, List<Filter> filters = null, List<Sort> sorts = null)
        => ToPagedList(query, new PaginationFilter(pageNumber, pageSize, filters, sorts));

    public static PagedList<T> ToPagedList<T>(this IQueryable<T> query, PaginationFilter paginationFilter)
        => new PagedList<T>(query, paginationFilter.Skip, paginationFilter.Take, paginationFilter.Filters, paginationFilter.Sorts);
}
