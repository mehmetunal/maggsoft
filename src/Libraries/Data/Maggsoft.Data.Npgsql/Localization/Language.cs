using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maggsoft.Data.Npgsql.Localization;

[Table("dev_Language")]
public partial class Language : BaseEntity, IPrimaryKey<Guid>
{
    public Language()
    {
        LocaleStringResources = new List<LocaleStringResource>();
    }

    [Required]
    [Column(Order = 8)]
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; }

    [Required]
    [Column(Order = 9)]
    /// <summary>
    /// Gets or sets the language culture
    /// </summary>
    public string LanguageCulture { get; set; }

    [Required]
    [Column(Order = 10)]
    /// <summary>
    /// Gets or sets the unique SEO code
    /// </summary>
    public string UniqueSeoCode { get; set; }

    [Required]
    [Column(Order = 11)]
    /// <summary>
    /// Gets or sets the flag image file name
    /// </summary>
    public string FlagImageFileName { get; set; }

    [DefaultValue(false)]
    [Column(Order = 12)]
    /// <summary>
    /// Gets or sets a value indicating whether the language supports "Right-to-left"
    /// </summary>
    public bool Rtl { get; set; }

    [Required]
    [Column(Order = 13)]
    /// <summary>
    /// Gets or sets a value indicating whether the language is published
    /// </summary>
    public int State { get; set; }

    [Required]
    [Column(Order = 14)]
    /// <summary>
    /// Gets or sets the display order
    /// </summary>
    public int DisplayOrder { get; set; }


    public virtual ICollection<LocaleStringResource> LocaleStringResources { get; set; }
}
