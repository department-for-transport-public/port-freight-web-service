using Microsoft.AspNetCore.Http;
using PortFreight.Services.Common;

namespace PortFreight.Services.Interface
{
    public interface IShipListBulkUpload
    {
        MethodResult BulkUploadShipList(IFormFile uploadedShipList, string uploadedByUserName );

    }
}
