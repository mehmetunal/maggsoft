using Maggsoft.Core.Model.Pagination;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Maggsoft.Core.Model.DataTables;

public class DataTablesRequest
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
    public Search Search { get; set; }
    public ColumnCollection Columns { get; set; }

    /// <summary>
    /// .NET Core 8+ için BindAsync metodu - ParameterInfo parametresi ile
    /// </summary>
    public static async ValueTask<DataTablesRequest?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var result = await DatatableModelBinder.BindModelAsync(context.Request.Query);
        return result;
    }

    /// <summary>
    /// Eski sürümler için geriye dönük uyumluluk - ParameterInfo parametresi olmadan
    /// </summary>
    public static ValueTask<DataTablesRequest> BindAsync(HttpContext context)
    {
        var result = DatatableModelBinder.BindModelAsync(context.Request.Query).GetAwaiter().GetResult();
        return ValueTask.FromResult(result);
    }

    public PaginationFilter ToPaginationFilter()
    {
        var sorts = new List<Sort>();
        var filters = new List<Filter>();

        if (!Columns.Any())
        {
            return new PaginationFilter(Start, Length, filters, sorts); ;
        }

        var sortedColumns = Columns.GetSortedColumns();
        var filteredColumns = Columns.GetFilteredColumns();

        sorts.AddRange(sortedColumns.Select(column => new Sort
            { Asc = (column.SortDirection == OrderDirection.Ascendant), Field = column.Data }));

        foreach (var column in filteredColumns)
        {
            var operatorType = "contains";
            var columnName = !string.IsNullOrEmpty(column.Name) ? column.Name : column.Data;
            var searchValue = column.Search.Value;
            
            // Date alanları için DateTime.TryParse kontrolü
            if (!string.IsNullOrEmpty(columnName) && columnName.Contains("date", StringComparison.OrdinalIgnoreCase) 
                && DateTime.TryParse(searchValue, out _))
            {
                operatorType = "equals";
            }
            
            filters.Add(new Filter 
            { 
                Field = columnName, 
                Operator = operatorType, 
                Value = searchValue 
            });
        }

        var fResult = new PaginationFilter(Start, Length, filters, sorts);

        return fResult;
    }

}
