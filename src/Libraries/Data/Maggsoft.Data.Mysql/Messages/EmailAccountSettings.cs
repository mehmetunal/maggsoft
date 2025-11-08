using System;
using Maggsoft.Core.IoC;

namespace Maggsoft.Data.Mysql.Messages;

/// <summary>
/// Email account settings
/// </summary>
public class EmailAccountSettings  : ISettings
{
    /// <summary>
    /// Gets or sets a store default email account identifier
    /// </summary>
    public Guid DefaultEmailAccountId { get; set; }
}
