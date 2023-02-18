using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maggsoft.Data.Npgsql.Enum
{
    public partial class Parameter : BaseEntity, IPrimaryKey<Guid>, ILocalizedEntity
    {
        public Parameter()
        {
            ParameterValue = new List<ParameterValue>();
        }

        [Required]
        [StringLength(200)]
        [Column(Order = 8)]
        public string Name { get; set; }

        public virtual ICollection<ParameterValue> ParameterValue { get; set; }
    }
}
