using System;

namespace Maggsoft.Data.Mongo.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BsonCollectionAttribute(string collectionName) : Attribute
{
    private readonly string _collectionName = collectionName;

    public string CollectionName => _collectionName;
}