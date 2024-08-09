using Maggsoft.Core.IoC;

namespace Maggsoft.Core.Messages;

/// <summary>
/// Email account settings
/// </summary>
public class EmailAccountSettings : ISettings
{
    /// <summary>
    /// Gets or sets a store default email account identifier
    /// </summary>
    public object DefaultEmailAccountId { get; set; }
}
