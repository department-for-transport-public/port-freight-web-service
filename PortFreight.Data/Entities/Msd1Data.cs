using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortFreight.Data.Entities
{
    public enum DataSource : int
    {
        WEB = 1,
        ASCII = 2,
        GESMES = 3,
    }

    public partial class Msd1Data
    {
        public Msd1Data()
        {
            Msd1CargoSummary = new HashSet<Msd1CargoSummary>();
        }
        public string Msd1Id { get; set; }
        public uint Imo { get; set; }
        public uint Year { get; set; }
        public ushort Quarter { get; set; }
        public string AssociatedPort { get; set; }
        public string ReportingPort { get; set; }
        public uint NumVoyages { get; set; }
        public bool IsInbound { get; set; }
        public DateTime? VoyageDate { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public uint DataSourceId { get; set; }
        public string AgentSenderId { get; set; }
        public string LineSenderId { get; set; }
        public string RecordRef { get; set; }
        public int? FileRefId { get; set; }
        public string LastUpdatedBy { get; set; }
        [NotMapped]
        public string ShipType { get; set; }
        
        public string ShipName { get; set; }
        public string FlagCode { get; set; }
        public string Callsign { get; set; }

        [NotMapped]
        public uint DeadWeight { get; set; }
        [NotMapped]
        public string SenderID { get; set; }

        public Msd1DataSource DataSource { get; set; }
        public ICollection<Msd1CargoSummary> Msd1CargoSummary { get; set; }
    }
}
