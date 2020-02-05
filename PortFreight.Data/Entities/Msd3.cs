using System;
using System.Collections.Generic;

namespace PortFreight.Data.Entities
{
    public partial class Msd3
    {
        public Msd3()
        {
            Msd3agents = new HashSet<Msd3agents>();
        }

        public string Id { get; set; }
        public string SenderId { get; set; }
        public string ReportingPort { get; set; }
        public uint Year { get; set; }
        public ushort Quarter { get; set; }
        public uint DataSourceId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
       
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public int? FileRefId { get; set; }        
        public ICollection<Msd3agents> Msd3agents { get; set; }
    }
}
