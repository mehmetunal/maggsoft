using System.Collections.Generic;
using Maggsoft.Core.Model.Pagination;
using System.Linq;
using System.Threading.Tasks;
using Maggsoft.Core.Model;
using Maggsoft.Core.Model.DataTables;

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
        => new(query, paginationFilter.Skip, paginationFilter.Take, paginationFilter.Filters, paginationFilter.Sorts);

    public static Task<DatatablePagedList<T>> ToDatatablePagedListAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize, List<Filter> filters = null, List<Sort> sorts = null)
        => Task.FromResult(query.ToDatatablePagedList(new PaginationFilter(pageNumber, pageSize, filters, sorts)));
    public static Task<DatatablePagedList<T>> ToDatatablePagedListAsync<T>(this IQueryable<T> query, PaginationFilter paginationFilter)
        => Task.FromResult(query.ToDatatablePagedList(paginationFilter));
    public static DatatablePagedList<T> ToDatatablePagedList<T>(this IQueryable<T> query, int pageNumber, int pageSize, List<Filter> filters = null, List<Sort> sorts = null)
        => query.ToDatatablePagedList(new PaginationFilter(pageNumber, pageSize, filters, sorts));
    public static DatatablePagedList<T> ToDatatablePagedList<T>(this IQueryable<T> query, PaginationFilter paginationFilter)
        => new(query, paginationFilter.Skip, paginationFilter.Take, paginationFilter.Filters, paginationFilter.Sorts);
}
