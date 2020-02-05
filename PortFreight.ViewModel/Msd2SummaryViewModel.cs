using System;

namespace PortFreight.ViewModel
{   
    public class  Msd2SummaryViewModel
    {
            public int Id { get; set; }
            public ushort Quarter { get; set; }
            public uint Year { get; set; }
            public string SenderId { get; set; }
            public string ReportingPort { get; set; }
            public decimal GrossWeightInward { get; set; }
            public uint TotalUnitInward { get; set; }
            public uint PassengerVehiclesInward { get; set; }
            public decimal GrossWeightOutward { get; set; }
            public uint TotalUnitOutward { get ; set; }
            public uint PassengerVehiclesOutward { get; set; }
            public string LastUpdatedBy { get; set; }
            public string StatisticalPort { get; set; }
            public string Source { get; set; }
            public DateTime? CreatedDate { get;  set; }
      
        }
}
