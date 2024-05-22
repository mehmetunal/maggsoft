using Maggsoft.Core.Extensions;
using Maggsoft.Core.Mapper;
using Maggsoft.Core.Model.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maggsoft.Core.Model.DataTables
{
    [Serializable]
    public class DatatablePagedList<T> : IPagedList<T>
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public bool HasPreviousPage => PageIndex > 0;

        public bool HasNextPage => PageIndex + 1 < TotalPages;

        public IEnumerable<T> Data { get; set; }

        public List<Filter> Filters { get; set; }

        public List<Sort> Sorts { get; set; }

        public DatatablePagedList(IQueryable<T> source, int pageIndex, int pageSize, List<Filter> filters = null, List<Sort> sorts = null, bool getOnlyTotalCount = false)
        {
            var q = source.AddFilterQuery(filters);
            int num2 = TotalCount = q.Count();
            TotalPages = num2 / pageSize;
            if (num2 % pageSize > 0)
            {
                TotalPages++;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
            if (!getOnlyTotalCount)
            {
                if (filters.Count() > 0 && pageIndex > num2)
                {
                    pageIndex = num2 - 1;
                }

                Filters = filters;
                Sorts = sorts;
                Data = source.AddFilterQuery(Filters).Skip(pageIndex).Take(pageSize)
                    .AddSortQuery(Sorts)
                    .ToList();
            }
        }

        public DatatablePagedList(IList<T> source, int pageIndex, int pageSize, List<Filter> filters = null, List<Sort> sorts = null)
        {
            TotalCount = source.AsQueryable().AddFilterQuery(filters).Count();
            TotalPages = TotalCount / pageSize;
            if (TotalCount % pageSize > 0)
            {
                TotalPages++;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
            if (filters.Count() > 0 && pageIndex > TotalCount)
            {
                pageIndex = TotalCount - 1;
            }

            Filters = filters;
            Sorts = sorts;
            Data = source.AsQueryable().AddFilterQuery(Filters).Skip(pageIndex)
                .Take(pageSize)
                .AddSortQuery(Sorts)
                .ToList();
        }

        public DatatablePagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount, List<Filter> filters = null, List<Sort> sorts = null)
        {
            TotalCount = totalCount;
            TotalPages = TotalCount / pageSize;
            if (TotalCount % pageSize > 0)
            {
                TotalPages++;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
            Filters = filters;
            Sorts = sorts;
            Data = source;
        }

        public IPagedList<TDestination> ToMap<TDestination>()
        {
            return AutoMapperConfiguration.Mapper.Map<IPagedList<T>, IPagedList<TDestination>>(this);
        }
    }
}
