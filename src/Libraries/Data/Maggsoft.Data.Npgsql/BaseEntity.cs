using Maggsoft.Core.Entities;
using System;

namespace Maggsoft.Data.Npgsql;

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
    public TKey Id { get; set; }
    public bool IsPublish { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatorIP { get; set; }
    public Guid CreatorUserId { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string UpdatedIP { get; set; }
    public Guid? UpdatedByUserId { get; set; } 
    public int DisplayOrder { get; set; }
}
