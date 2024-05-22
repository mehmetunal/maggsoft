using System;

namespace Maggsoft.Core.Model.DataTables;

public class Column
{
    public string Data { get; private set; }

    public string Name { get; private set; }

    public bool Searchable { get; private set; }

    public bool Orderable { get; private set; }

    public Search Search { get; private set; }

    public bool IsOrdered => OrderNumber != -1;

    public int OrderNumber { get; private set; }

    public OrderDirection SortDirection { get; private set; }

    public void SetColumnOrdering(int orderNumber, string orderDirection)
    {
        OrderNumber = orderNumber;
        if (orderDirection.ToLower().Equals("asc"))
        {
            SortDirection = OrderDirection.Ascendant;
            return;
        }

        if (orderDirection.ToLower().Equals("desc"))
        {
            SortDirection = OrderDirection.Descendant;
            return;
        }

        throw new ArgumentException("The provided ordering direction was not valid. Valid values must be 'asc' or 'desc' only.");
    }

    public Column(string data, string name, bool searchable, bool orderable, string searchValue, bool isRegexValue)
    {
        Data = data;
        Name = name;
        Searchable = searchable;
        Orderable = orderable;
        Search = new Search(searchValue, isRegexValue);
        OrderNumber = -1;
    }
}
