using Microsoft.AspNetCore.Http;

namespace PortFreight.Services.Interface
{
    public interface IFileUploadValidator
    {

        bool FileContentTypeCSV(string contentType);
        
        bool ValidFileHeader(string requiredHeader,string uploadedHeader);
                
    }
}
