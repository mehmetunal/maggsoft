using Maggsoft.Core.IoC;
using MongoDB.Bson;

namespace Maggsoft.Data.Mongo.Messages;

/// <summary>
/// Email account settings
/// </summary>
public class EmailAccountSettings : ISettings
{
    /// <summary>
    /// Gets or sets a store default email account identifier
    /// </summary>
    public ObjectId DefaultEmailAccountId { get; set; }
}
