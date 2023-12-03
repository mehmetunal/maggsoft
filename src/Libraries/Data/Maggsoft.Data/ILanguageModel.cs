using System.ComponentModel.DataAnnotations;

namespace Maggsoft.Data;

public interface ILanguageModel<T>
{
    [Required]
    T LanguageId { get; set; }

    [Required]
    [StringLength(200)]
    string Content { get; set; }
}
