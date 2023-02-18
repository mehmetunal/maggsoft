using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maggsoft.Data.Npgsql.Location
{
    public class City : BaseEntity, IPrimaryKey<Guid>
    {
        [Required]
        [Column(Order = 8)]
        public decimal State { get; set; }

        [Required]
        [Column(Order = 9)]
        public decimal CountryId { get; set; }

        [Required]
        [StringLength(200)]
        [Column(Order = 10)]
        public string Name { get; set; }


        public virtual Country Country { get; set; }
        public virtual ICollection<County> Counties { get; set; }
    }
}
