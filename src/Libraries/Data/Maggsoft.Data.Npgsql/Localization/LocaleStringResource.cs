using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maggsoft.Data.Npgsql.Localization;

[Table("dev_LocaleStringResource")]
public partial class LocaleStringResource : BaseEntity, IPrimaryKey<Guid>
{
    [Required]
    [Column(Order = 8)]
    /// <summary>
    /// Gets or sets the language identifier
    /// </summary>
    public Guid LanguageId { get; set; }

    [Required]
    [Column(Order = 9)]
    /// <summary>
    /// Gets or sets the resource name
    /// </summary>
    public string ResourceName { get; set; }

    [Required]
    [Column(Order = 10)]
    /// <summary>
    /// Gets or sets the resource value
    /// </summary>
    public string ResourceValue { get; set; }


    public virtual Language Language { get; set; }
}
