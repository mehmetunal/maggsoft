using System;
using MongoDB.Bson;
using Maggsoft.Core.Entities;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Maggsoft.Data.Mongo;


public abstract class BaseEntity : Data.BaseEntity, IBaseEntity, IEntity   
{
    protected BaseEntity()
    {
        _id = ObjectId.GenerateNewId();
        CreatedDate = DateTime.UtcNow;
    }

    [BsonId]
    [DataMember]
    private ObjectId _id;

    [DataMember]
    public ObjectId Id
    {
        get => _id;
        set => _id = value;
    }

    
    [DataMember]
    public bool IsPublish { get; set; }
    [DataMember]
    public bool IsDeleted { get; set; }
    [DataMember]
    public DateTime CreatedDate { get; set; }
    [DataMember]
    public string CreatorIP { get; set; }
    [DataMember]
    public string CreatorUserId { get; set; }
    [DataMember]
    public DateTime? ModifiedDate { get; set; }
    [DataMember]
    public string ModifierIP { get; set; }
    [DataMember]
    public string ModifierUserId { get; set; }
    [DataMember]
    public int DisplayOrder { get; set; }
}
