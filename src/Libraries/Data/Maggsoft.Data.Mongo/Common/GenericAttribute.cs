using Maggsoft.Data.Mongo.Attributes;
using MongoDB.Bson;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maggsoft.Data.Mongo.Common
{
    [BsonCollection("dev_GenericAttribute")]
    public partial class GenericAttribute : BaseEntity, IPrimaryKey<ObjectId>
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the key group
        /// </summary>
        public string KeyGroup { get; set; }

        /// <summary>
        /// Gets or sets the key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the created or updated date
        /// </summary>
        public DateTime? CreatedOrUpdatedDateUTC { get; set; }
    }
}
