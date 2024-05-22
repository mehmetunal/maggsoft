using Maggsoft.Core.Model.Pagination;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maggsoft.Core.Model.DataTables;

public class DataTablesRequest
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
    public Search Search { get; set; }
    public ColumnCollection Columns { get; set; }


    public static ValueTask<DataTablesRequest> BindAsync(HttpContext context)
    {
        var result = DatatableModelBinder.BindModelAsync(context.Request.Query).GetAwaiter().GetResult();
        return ValueTask.FromResult(result);
    }

    public PaginationFilter ToPaginationFilter()
    {
        var sorts = new List<Maggsoft.Core.Model.Sort>();
        var filters = new List<Maggsoft.Core.Model.Filter>();

        if (this.Columns == null || this.Columns.Count() == 0)
        {
            return new PaginationFilter(this.Start, this.Length, filters, sorts); ;
        }

        var sortedColumns = this.Columns.GetSortedColumns();
        var filteredColumns = this.Columns.GetFilteredColumns();

        foreach (var column in sortedColumns)
        {
            sorts.Add(new Maggsoft.Core.Model.Sort { Asc = (column.SortDirection == OrderDirection.Ascendant), Field = column.Data });
        }

        foreach (var column in filteredColumns)
        {
            filters.Add(new Maggsoft.Core.Model.Filter { Field = !string.IsNullOrEmpty(column.Name) ? column.Name : column.Data, Operator = "contains", Value = column.Search.Value });
        }

        var fResult = new PaginationFilter(this.Start, this.Length, filters, sorts);

        return fResult;
    }

}
