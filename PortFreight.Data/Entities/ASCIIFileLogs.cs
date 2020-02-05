using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortFreight.Data.Entities
{
    public partial class FileUploadInfo
    {
        [NotMapped]
        public int Id { get; set; }
        public string FileName { get; set; }
        public string UploadBy { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
