using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maggsoft.Dto.Npgsql.Identity.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public Guid ObjectId { get; set; }
        public decimal State { get; set; }
        public decimal AccessLevel { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal LanguageId { get; set; }

        public virtual ICollection<UserRoleDto> UserRoleDto { get; set; }
    }
}
