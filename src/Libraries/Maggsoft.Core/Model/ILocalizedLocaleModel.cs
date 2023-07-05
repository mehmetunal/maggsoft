using System;

namespace Maggsoft.Core.Model
{
    public interface ILocalizedLocaleModel : ILocalizedLocaleModel<Guid>
    {
    }

    /// <summary>
    /// Represents localized locale model
    /// </summary>
    public interface ILocalizedLocaleModel<T>
    {
        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        T LanguageId { get; set; }
    }
}
