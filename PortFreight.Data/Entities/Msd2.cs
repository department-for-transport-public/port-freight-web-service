using System;
using System.Collections.Generic;

namespace PortFreight.Data.Entities
{
    public partial class Msd2
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReportingPort { get; set; }
        public uint Year { get; set; }
        public ushort Quarter { get; set; }
        public decimal GrossWeightInward { get; set; }
        public string InwardGrossWeightDescription { get; set; }
        public uint TotalUnitsInward { get; set; }
        public string InwardUnitDescription { get; set; }
        public uint PassengerVehiclesInward { get; set; }
        public decimal GrossWeightOutward { get; set; }
        public string OutwardGrossWeightDescription { get; set; }
        public uint TotalUnitsOutward { get; set; }
        public string OutwardUnitDescription { get; set; }
        public uint PassengerVehiclesOutward { get; set; }
        public uint DataSourceId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public int? FileRefId { get; set; }
    }
}
