using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Maggsoft.Core.Model.DataTables;

public class ColumnCollection : IEnumerable<Column>, IEnumerable
{
    private IReadOnlyList<Column> Data;

    public ColumnCollection(IEnumerable<Column> columns)
    {
        if (columns == null)
        {
            throw new ArgumentNullException("The provided column collection cannot be null", "columns");
        }

        Data = columns.ToList().AsReadOnly();
    }

    public IOrderedEnumerable<Column> GetSortedColumns()
    {
        return from _column in Data
               where !string.IsNullOrWhiteSpace(_column.Data) && _column.IsOrdered
               select _column into _c
               orderby _c.OrderNumber
               select _c;
    }

    public IEnumerable<Column> GetFilteredColumns()
    {
        return Data.Where((Column _column) => !string.IsNullOrWhiteSpace(_column.Data) && _column.Searchable && !string.IsNullOrWhiteSpace(_column.Search.Value));
    }

    public IEnumerator<Column> GetEnumerator()
    {
        return Data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Data).GetEnumerator();
    }
}
