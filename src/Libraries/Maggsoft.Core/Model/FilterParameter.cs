using Maggsoft.Core.Model.ModelBinder;
using System.Collections.Generic;

namespace Maggsoft.Core.Model;

public class FilterParameter
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = int.MaxValue;

    [FromJsonQuery]
    /// <summary>
    /// Data Filter
    /// </summary>
    public List<Filter> Filters { get; set; } = new List<Filter>();

    [FromJsonQuery]
    /// <summary>
    /// Data Sort OrderBy and OrderBy Desc
    /// </summary>
    public List<Sort> Sorts { get; set; } = new List<Sort>();
}
