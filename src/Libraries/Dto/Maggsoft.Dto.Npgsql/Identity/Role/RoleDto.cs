using System;
using System.Collections.Generic;
using Maggsoft.Dto.Npgsql.Identity.User;

namespace Maggsoft.Dto.Npgsql.Identity.Role
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public Guid ObjectId { get; set; }
        public decimal State { get; set; }
        public decimal AccessLevel { get; set; }
        public string Name { get; set; }

        public virtual ICollection<UserRoleDto> UserRoleDto { get; set; }
    }
}
