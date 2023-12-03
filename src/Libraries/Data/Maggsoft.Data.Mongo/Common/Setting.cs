using Maggsoft.Data.Mongo.Attributes;
using MongoDB.Bson;

namespace Maggsoft.Data.Mongo.Common;

/// <summary>
/// dev_setting
/// </summary>
[BsonCollection("dev_setting")]
public partial class Setting : BaseEntity, IPrimaryKey<ObjectId>
{
    public Setting()
    {
    }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="name">Name</param>
    /// <param name="value">Value</param>
    /// <param name="storeId">Store identifier</param>
    public Setting(string name, string value, int storeId = 0)
    {
        Name = name;
        Value = value;
        StoreId = storeId;
    }

    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets the store for which this setting is valid. 0 is set when the setting is for all stores
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// To string
    /// </summary>
    /// <returns>Result</returns>
    public override string ToString()
    {
        return Name;
    }
}