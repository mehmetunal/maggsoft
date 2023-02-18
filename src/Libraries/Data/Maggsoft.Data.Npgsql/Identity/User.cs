using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maggsoft.Data.Npgsql.Identity
{
    public class User : BaseEntity, IPrimaryKey<decimal>
    {
        public User()
        {
            UserToken = new List<UserToken>();
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
        public string FirstName { get; set; }

        [Required]
        [StringLength(200)]
        [Column(Order = 11)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        [Column(Order = 12)]
        public string Email { get; set; }

        [Required]
        [StringLength(200)]
        [PasswordPropertyText]
        [Column(Order = 13)]
        public string Password { get; set; }

        [Required]
        [Column(Order = 14)]
        public Guid LanguageId { get; set; }

        public virtual ICollection<UserToken> UserToken { get; set; }
        public virtual ICollection<UserRole> UserRole { get; set; }
    }
}
