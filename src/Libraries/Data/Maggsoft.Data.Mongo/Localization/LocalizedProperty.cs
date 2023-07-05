using Maggsoft.Data.Mongo.Attributes;
using MongoDB.Bson;
using System;
using System.ComponentModel.DataAnnotations;

namespace Maggsoft.Data.Mongo.Localization
{
    [BsonCollection("dev_localized_property")]
    public class LocalizedProperty : BaseEntity, IPrimaryKey<ObjectId>
    {
        [Required]
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public ObjectId EntityId { get; set; }

        [Required]
        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        public ObjectId LanguageId { get; set; }

        [Required]
        /// <summary>
        /// Gets or sets the locale key group
        /// </summary>
        public string LocaleKeyGroup { get; set; }

        [Required]
        /// <summary>
        /// Gets or sets the locale key
        /// </summary>
        public string LocaleKey { get; set; }

        [Required]
        /// <summary>
        /// Gets or sets the locale value
        /// </summary>
        public string LocaleValue { get; set; }

        public virtual Language Language { get; set; }
    }
}
