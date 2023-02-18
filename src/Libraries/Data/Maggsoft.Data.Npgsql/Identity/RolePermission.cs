using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maggsoft.Data.Npgsql.Identity
{
    [Table("dev_RolePermission")]
    public class RolePermission : BaseEntity, IPrimaryKey<Guid>
    {
        [Required]
        [MaxLength(18)]
        [Column(Order = 8)]
        public Guid RoleId { get; set; }

        [Required]
        [MaxLength(18)]
        [Column(Order = 9)]
        public Guid PermissionId { get; set; }

        public virtual Role Role { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
