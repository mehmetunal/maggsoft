using AutoMapper.QueryableExtensions;
using Maggsoft.Core.Extensions;
using Maggsoft.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maggsoft.Core.Model.Pagination
{
    /// <summary>
    /// Paged list
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    [Serializable]
    public class PagedList<T> : IPagedList<T>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="skip">Skip</param>
        /// <param name="take">Take</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        /// <param name="filters">Data Filter </param>
        /// <param name="sorts">Data Sort OrderBy and OrderBy Desc</param>
        public PagedList(IQueryable<T> source, int pageIndex, int pageSize, List<Filter> filters = null, List<Sort> sorts = null, bool getOnlyTotalCount = false)
        {
            var total = source.AddFilterQuery(filters).Count();
            TotalCount = total;
            TotalPages = total / pageSize;

            if (total % pageSize > 0)
                TotalPages++;

            PageSize = pageSize;
            PageIndex = pageIndex;
            if (getOnlyTotalCount)
                return;

            if (filters.Count() > 0 && pageIndex > total)
                pageIndex = total - 1;

            Filters = filters;
            Sorts = sorts;

            #region dxGrid
            //Data = source.AddFilterQuery(Filters).Skip(pageIndex).Take(pageSize).AddSortQuery(Sorts).ToList();
            #endregion

            #region NormalPage
            Data = source.AddFilterQuery(Filters).Skip(pageIndex * pageSize).Take(pageSize).AddSortQuery(Sorts).ToList();
            #endregion
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="skip">Skip</param>
        /// <param name="take">Take</param>
        /// <param name="filters">Data Filter </param>
        /// <param name="sorts">Data Sort OrderBy and OrderBy Desc</param>
        public PagedList(IList<T> source, int pageIndex, int pageSize, List<Filter> filters = null, List<Sort> sorts = null)
        {
            TotalCount = source.AsQueryable().AddFilterQuery(filters).Count();
            TotalPages = TotalCount / pageSize;

            if (TotalCount % pageSize > 0)
                TotalPages++;

            PageSize = pageSize;
            PageIndex = pageIndex;

            if (filters.Count() > 0 && pageIndex > TotalCount)
                pageIndex = TotalCount - 1;

            Filters = filters;
            Sorts = sorts;

            Data = source.AsQueryable().AddFilterQuery(Filters).Skip(pageIndex * pageSize).Take(pageSize).AddSortQuery(Sorts).ToList();
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="skip">Skip</param>
        /// <param name="take">Take</param>
        /// <param name="totalCount">Total count</param>
        /// <param name="filters">Data Filter </param>
        /// <param name="sorts">Data Sort OrderBy and OrderBy Desc</param>
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount, List<Filter> filters = null, List<Sort> sorts = null)
        {
            TotalCount = totalCount;
            TotalPages = TotalCount / pageSize;

            if (TotalCount % pageSize > 0)
                TotalPages++;

            PageSize = pageSize;
            PageIndex = pageIndex;

            Filters = filters;
            Sorts = sorts;

            Data = source;
        }


        /// <summary>
        /// PageIndex
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// PageSize
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total pages
        /// </summary>
        public int TotalPages { get; set; }


        /// <summary>
        /// Has previous page
        /// </summary>
        public bool HasPreviousPage => PageIndex > 0;

        /// <summary>
        /// Has next page
        /// </summary>
        public bool HasNextPage => PageIndex + 1 < TotalPages;

        /// <summary>
        /// DB Data
        /// </summary>
        public IEnumerable<T> Data { get; set; }

        /// <summary>
        /// Data Filter
        /// </summary>
        public List<Filter> Filters { get; set; }

        /// <summary>
        /// Data Sort OrderBy and OrderBy Desc
        /// </summary>
        public List<Sort> Sorts { get; set; }
    }
}
