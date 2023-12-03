using System.Collections.Generic;

namespace Maggsoft.Core.Model.Pagination;

public class PaginationFilter
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 50;

    private int _skip = 0;
    private int _take = DefaultPageSize;

    private List<Filter> _filters;
    private List<Sort> _sorts;

    public PaginationFilter()
    {
    }

    public PaginationFilter(int skip, int take, List<Filter> filters, List<Sort> sorts)
    {
        Skip = skip;
        Take = take;
        Filters = filters;
        Sorts = sorts;
    }

    public int Skip
    {
        get => _skip;
        set => _skip = value < 0 ? 1 : value;
    }

    public int Take
    {
        get => _take;
        set => _take = value < 0
            ? DefaultPageSize
            : value > MaxPageSize
                ? MaxPageSize
                : value;
    }

    /// <summary>
    /// Data Filter
    /// </summary>
    public List<Filter> Filters
    {
        get => _filters;
        set => _filters = value == null ? new List<Filter>() : value;
    }

    /// <summary>
    /// Data Sort OrderBy and OrderBy Desc
    /// </summary>
    public List<Sort> Sorts
    {
        get => _sorts;
        set => _sorts = value == null ? new List<Sort>() : value;
    }
}