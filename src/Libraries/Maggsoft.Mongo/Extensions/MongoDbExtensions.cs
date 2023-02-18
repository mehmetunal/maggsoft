using System;
using System.Linq;
using Maggsoft.Data.Mongo.Attributes;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maggsoft.Mongo.Extensions
{
    public static class MongoDbExtensions
    {
        /// <summary>
        /// mongodb://<username>:<password>@<server_address>:<port>/<database_name>
        /// use mydiary
        /// db.createUser(
        /// {
        ///     user: "mehmet",
        ///     pwd: "123",
        ///     roles:
        ///     [
        ///         { role: "root", db: "admin" }
        ///     ]
        /// })
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddMongoDbConfig(this IServiceCollection services,
            IConfiguration configuration)
        {
            var mongoDbConnection = configuration.GetSection("ConnectionStrings:Mongo").Value;
            if (string.IsNullOrEmpty(mongoDbConnection)) return services;

            var client = new MongoClient(mongoDbConnection);

            var databaseName = new MongoUrl(mongoDbConnection)?.DatabaseName;
            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = configuration.GetSection("ConnectionStrings:Database").Value;
                if (client != null && client.Settings != null)
                {
                    var setting = client.Settings.Clone();
                    if (client.Settings.Credential != null)
                    {
                        setting.Credential = MongoCredential.CreateCredential(databaseName, client.Settings.Credential.Username, client.Settings.Credential.Password);
                    }

                    client = new MongoClient(setting);
                }
            }

            services.AddScoped(typeof(IMongoDatabase), c => client.GetDatabase(databaseName));

            return services;
        }
        
        public static string GetCollectionName(this Type type)
        {
            return (type.GetCustomAttributes(typeof(BsonCollectionAttribute), true).FirstOrDefault()
                as BsonCollectionAttribute).CollectionName;
        }
    }
}
