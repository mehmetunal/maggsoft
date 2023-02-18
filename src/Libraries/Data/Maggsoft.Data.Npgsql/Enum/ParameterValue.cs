using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maggsoft.Data.Npgsql.Enum
{
    public partial class ParameterValue : BaseEntity, IPrimaryKey<Guid>, ILocalizedEntity
    {
        [Required]
        [Column(Order = 8)]
        public decimal State { get; set; }

        [Required]
        [Column(Order = 9)]
        public decimal AccessLevel { get; set; }

        [Required]
        [Column(Order = 10)]
        public decimal ParameterId { get; set; }

        [Required]
        [StringLength(200)]
        [Column(Order = 11)]
        public string Name { get; set; }

        [Required]
        [Column(Order = 12)]
        public decimal Value { get; set; }

        public virtual Parameter Parameter { get; set; }
    }
}
