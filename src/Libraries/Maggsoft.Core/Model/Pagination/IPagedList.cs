using System.Collections.Generic;

namespace Maggsoft.Core.Model.Pagination
{
    /// <summary>
    /// Paged list interface
    /// </summary>
    public interface IPagedList<T>
    {
        /// <summary>
        /// PageIndex
        /// </summary>
        int PageIndex { get; }

        /// <summary>
        /// Take
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Total count
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Total pages
        /// </summary>
        int TotalPages { get; }

        /// <summary>
        /// Has previous page
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        /// Has next age
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        /// Data
        /// </summary>
        IEnumerable<T> Data { get; set; }

        /// <summary>
        /// Data Filter
        /// </summary>
        List<Filter> Filters { get; set; }

        /// <summary>
        /// Data Sort OrderBy and OrderBy Desc
        /// </summary>
        List<Sort> Sorts { get; set; }
    }
}
