using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maggsoft.Data.Npgsql.Identity
{
    [Table("dev_UserRole")]
    public class UserRole : BaseEntity, IPrimaryKey<Guid>
    {
        [Required]
        [MaxLength(18)]
        [Column(Order = 8)]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(18)]
        [Column(Order = 9)]
        public Guid RoleId { get; set; }

        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}
