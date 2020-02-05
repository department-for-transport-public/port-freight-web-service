using Google.Cloud.Datastore.V1;
using PortFreight.Data;
using PortFreight.Data.Entities;
using System;
using System.Threading.Tasks;

namespace PortFreight.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IApiKeyDataStore _context;

        public ApiKeyService(IApiKeyDataStore context) => _context = context;

        public  ApiKey GetApiKey(string id)
        {
            return _context.Get(id);
        }

        public bool CreateApiKey(string id, DataSource source)
        {
            var apiKey = new ApiKey
            {
                Id = id,
                Token = Guid.NewGuid().ToString(),
                Source = Enum.GetName(typeof(DataSource), source)
            };

            if (apiKey != null)
            {
                _context.Upsert(apiKey);
                return true;
            }

            return false;
        }

    }
}
