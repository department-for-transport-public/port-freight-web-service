using PortFreight.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PortFreight.Services
{
    public interface IApiKeyService
    {
      ApiKey GetApiKey(string id);
      bool CreateApiKey(string id, DataSource source);
    }
}
