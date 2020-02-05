using System.Collections.Generic;


namespace PortFreight.Services.FleUpload
{
    public class UploadedFile
    {
        public List<string> dataRows ;
        public string contentType;
        public string fileName;
        public string headerRowData;
        public int NumberOfDataRows { get { return dataRows.Count; } }

        public UploadedFile()
        {
            dataRows = new List<string>();            
        }
    }
}
