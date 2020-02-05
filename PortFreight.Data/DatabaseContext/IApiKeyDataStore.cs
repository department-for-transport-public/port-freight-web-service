using Google.Cloud.Datastore.V1;
using PortFreight.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PortFreight.Data
{   
    public interface IApiKeyDataStore
    {        
        void Upsert(ApiKey apiKey);

        ApiKey Get(string id);        

        void Delete(string id);
    }
}
