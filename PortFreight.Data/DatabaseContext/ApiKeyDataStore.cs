using Google.Cloud.Datastore.V1;
using Microsoft.Extensions.Configuration;
using PortFreight.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Data
{
    public static class ApiKeyDataStoreExtensionMethods
    {

        public static Key ToKey(this string id) =>
            new Key().WithElement("ApiKey", id);


        public static Entity ToEntity(this ApiKey apiKey) => new Entity()
        {
            ["Id"] = apiKey.Id,
            ["Token"] = apiKey.Token,
            ["Source"] = apiKey.Source
        };

        public static ApiKey ToApiKey(this Entity entity) => new ApiKey()
        {
            Id = (string)entity["Id"],
            Token = (string)entity["Token"]
        };
    }

    public class ApiKeyDataStore : IApiKeyDataStore
    {
        private readonly IConfiguration _config;
        private readonly DatastoreDb _db;


        public ApiKeyDataStore(IConfiguration config)
        {
            _config = config;
            string projectId = _config["GOOGLE_CLOUD_PROJECT"] ?? _config["DataStore:ProjectId"];
            _db = DatastoreDb.Create(projectId);
        }


        public void Upsert(ApiKey apiKey)
        {
            var entity = apiKey.ToEntity();
            using (DatastoreTransaction transaction = _db.BeginTransaction())
            {
                entity.Key = _db.CreateKeyFactory("ApiKey").CreateKey(apiKey.Id);
                transaction.Upsert(entity);
                transaction.Commit();
            }
        }

        public void Delete(string id)
        {
            _db.Delete(id.ToKey());
        }

        public ApiKey Get(string id)
        {

            Query query = new Query("ApiKey")
            {
                Filter = Filter.Equal("Id",id)
            };

            var results = _db.RunQuery(query);
            var apiKey = results.Entities.Select(entity => entity.ToApiKey()).FirstOrDefault();

            return apiKey;

        }
    }
}
