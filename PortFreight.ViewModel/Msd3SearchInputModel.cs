using System;


namespace PortFreight.ViewModel
{    public class Msd3SearchInputModel
        {
            public string SenderId { get; set; }
            public string UniqueRef { get; set; }
            public string ReportingPort { get; set; }
            public string Year { get; set; }
            public string Quarter { get; set; }
            public string Agent { get; set; }
            public string DateEntered { get; set; }
            public string LastUpdatedBy { get; set; }
            public string StatisticalPort { get; set; }
            public string Source { get; set; }
            
            public Msd3SearchInputModel()
            {
            }
        }
}
