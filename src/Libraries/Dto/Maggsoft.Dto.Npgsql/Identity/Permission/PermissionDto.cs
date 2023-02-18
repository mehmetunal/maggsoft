using System;

namespace Maggsoft.Dto.Npgsql.Identity.Permission
{
    public class PermissionDto
    {
        public Guid Id { get; set; }
        public Guid ObjectId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatorIP { get; set; }
        public Guid CreatorUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifierIP { get; set; }
        public Guid? ModifierUserId { get; set; }
        public decimal State { get; set; }
        public decimal AccessLevel { get; set; }
        public string Name { get; set; }
    }
}
