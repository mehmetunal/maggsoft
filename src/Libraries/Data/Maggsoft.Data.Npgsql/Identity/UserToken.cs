using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maggsoft.Data.Npgsql.Identity
{
    public class UserToken : BaseEntity, IPrimaryKey<Guid>
    {
        [StringLength(300)]
        [Required]
        [Column(Order = 8)]
        public string RefreshToken { get; set; }

        [Column(Order = 9)]
        public DateTime? RefreshTokenEndDate { get; set; }

        [MaxLength(18)]
        [Required]
        [Column(Order = 10)]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }
    }
}
