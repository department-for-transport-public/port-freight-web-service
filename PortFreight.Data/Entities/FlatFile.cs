using System;

namespace PortFreight.Data.Entities
{
    public partial class FlatFile
    {       
        public int FileRefId { get; set; }
        public string FileName { get; set; }
        public string SenderId { get; set; }
        public DateTime? CreationDate { get; set; }
        public int? TotalRecords { get; set; }
        public string TableRef { get; set; }
        public sbyte? IsAmendment { get; set; }
        public sbyte? IsTest { get; set; }
        public int? RegistrationNumber { get; set; }
        public string SendersRef { get; set; }
    }
}
