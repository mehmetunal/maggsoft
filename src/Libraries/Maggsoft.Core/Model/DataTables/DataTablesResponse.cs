using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Maggsoft.Core.Model.DataTables;

public class DataTablesResponse<T>(int draw, T data, int recordsFiltered, int recordsTotal) where T : class
{
    [DataMember(Order = 1, Name = "Draw"), JsonProperty(PropertyName = "draw")]
    public int Draw { get; private set; } = draw;

    [DataMember(Order = 2, Name = "Data"), JsonProperty(PropertyName = "data")]
    public T Data { get; private set; } = data;

    [DataMember(Order = 3, Name = "RecordsTotal"), JsonProperty(PropertyName = "recordsTotal")]
    public int RecordsTotal { get; private set; } = recordsTotal;

    [DataMember(Order = 4, Name = "RecordsFiltered"), JsonProperty(PropertyName = "recordsFiltered")]
    public int RecordsFiltered { get; private set; } = recordsFiltered;
}
