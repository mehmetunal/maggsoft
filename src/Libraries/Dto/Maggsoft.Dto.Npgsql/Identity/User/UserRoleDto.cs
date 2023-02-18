using Maggsoft.Dto.Npgsql.Identity.Role;
using System;

namespace Maggsoft.Dto.Npgsql.Identity.User
{
    public class UserRoleDto
    {
        public decimal Id { get; set; }
        public Guid ObjectId { get; set; }
        public decimal UserId { get; set; }
        public decimal RoleId { get; set; }

        public virtual RoleDto RoleDto { get; set; }
    }
}