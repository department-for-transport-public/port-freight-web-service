using System;


namespace PortFreight.ViewModel
{    public class Msd2SearchInputModel
        {
            public string SenderId { get; set; }
            public string ReportingPort { get; set; }
            public string Year { get; set; }
            public string Quarter { get; set; }
            public string GrossWeightInward { get; set; }
            public string TotalUnitInward { get; set; }
            public string PassengerVehiclesInward { get; set; }
            public string FileRefId { get; set; }
            public string GrossWeightOutward { get; set; }
            public string TotalUnitOutward { get ; set; }
            public string PassengerVehiclesOutward { get; set; }
            public string DateEntered { get; set; }
            public string LastUpdatedBy { get; set; }
            public string StatisticalPort { get; set; }
            public string Source { get; set; }
            
            public Msd2SearchInputModel()
            {
            }
        }
}
