using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maggsoft.Data.Npgsql.Identity
{
    public partial class Role : BaseEntity, IPrimaryKey<Guid>, ILocalizedEntity
    {
        public Role()
        {
            RolePermission = new List<RolePermission>();
            UserRole = new List<UserRole>();
        }
        [Required]
        [Column(Order = 8)]
        public decimal State { get; set; }

        [Required]
        [Column(Order = 9)]
        public decimal AccessLevel { get; set; }

        [Required]
        [StringLength(200)]
        [Column(Order = 10)]
        public string Name { get; set; }

        public virtual ICollection<RolePermission> RolePermission { get; set; }
        public virtual ICollection<UserRole> UserRole { get; set; }
    }
}
