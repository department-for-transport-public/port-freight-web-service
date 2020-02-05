using Microsoft.AspNetCore.Http;
using PortFreight.Services.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PortFreight.Services.ShipListBulkUpload
{
    public class FileUploadValidator : IFileUploadValidator
    {        

        public bool FileContentTypeCSV(string uploadedFileContentType)
        {
            List<string> csvContentTypes = new List<string> { "text/comma-separated-values", "text/csv", "application/csv", "application/excel", "application/vnd.ms-excel", "application/vnd.msexcel", "text/anytext" };
            return csvContentTypes.Contains(uploadedFileContentType);
        }
        
        public bool ValidFileHeader(string requiredHeader, string uploadedFileHeader)
        {
            return string.Equals(uploadedFileHeader, requiredHeader, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
