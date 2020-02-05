using System;

namespace PortFreight.ViewModels
{
    public class Msd1SummaryViewModel
    {
        public string SenderId { get; set; }
        public string Agent { get; set; }
        public string Line { get; set; }
        // ReSharper disable once InconsistentNaming
        public uint IMO { get; set; }
        public string ShipName { get; set; }
        public string ShipType { get; set; }
        public uint Year { get; set; }
        public ushort Quarter { get; set; }
        public string ReportingPort { get; set; }
        public string LoadUnloadPort { get; set; }
        public string Direction { get; set; }
        public uint NumVoyages { get; set; }
        public DateTime? VoyageDate { get; set; }
        public byte? CargoCategory { get; set; }
        public decimal? GrossWeight { get; set; }
        public uint? UnitsWithCargo { get; set; }
        public uint? UnitsWithoutCargo { get; set; }
        public uint? TotalUnits { get; set; }
        public string Description { get; set; }
        public string DataSource { get; set; }
        public string SendersRecordRef { get; set; }
        public string SendersSubmissionRef { get; set; }
        public string OurRef { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public uint DataSourceId { get; set; }
        public int? FileRefId { get; set; }
    }
}
