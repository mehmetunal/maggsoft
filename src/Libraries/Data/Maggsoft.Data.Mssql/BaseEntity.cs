using Maggsoft.Core.Entities;
using System;

namespace Maggsoft.Data.Mssql
{
    [Serializable]
    public class BaseEntity : BaseEntity<Guid> 
    {
    }

    [Serializable]
    public abstract class BaseEntity<TKey> : Data.BaseEntity, IBaseEntity<TKey>, IEntity
    { 
        public BaseEntity()
        {
            CreatedDate = DateTime.UtcNow;
        }
        public virtual TKey Id { get; set; }
        public bool IsPublish { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatorIP { get; set; }
        public Guid CreatorUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifierIP { get; set; }
        public Guid? ModifierUserId { get; set; }
        public int DisplayOrder { get; set; }

        public void SoftDelete()
        {
            IsPublish = false;
            IsDeleted = true;
        }
    }
}
