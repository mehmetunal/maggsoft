using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Maggsoft.Data;

public interface IBaseEntity 
{
}
public interface IBaseEntity<TKey> : IBaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column(Order = 0)]
    TKey Id { get; set; }

    [DataMember]
    [Required]
    [Column(Order = 1)]
    DateTime CreatedDate { get; set; }

    [DataMember]
    [Required]
    [StringLength(50)]
    [Column(Order = 2)]
    string CreatorIP { get; set; }

    [DataMember]
    [Required]
    [MaxLength(18)]
    [Column(Order = 3)]
    Guid CreatorUserId { get; set; }

    [DataMember]
    [Column(Order = 4)]
    DateTime? ModifiedDate { get; set; }

    [DataMember]
    [StringLength(50)]
    [Column(Order = 5)]
    string ModifierIP { get; set; }

    [DataMember]
    [MaxLength(18)]
    [Column(Order = 6)]
    Guid? ModifierUserId { get; set; }
}
