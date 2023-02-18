using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maggsoft.Dto.Npgsql.Identity.Role
{
    public class RolePermissionDto
    {
        public Guid Id { get; set; }
        public Guid ObjectId { get; set; }
        public decimal RoleId { get; set; }
        public decimal PermissionId { get; set; }
    }
}
