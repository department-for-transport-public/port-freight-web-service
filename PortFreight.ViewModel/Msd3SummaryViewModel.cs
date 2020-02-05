using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace PortFreight.ViewModel
{   
    public class  Msd3SummaryViewModel
    {
            public string SenderId { get; set; }
            public string Id { get; set; }
            public string ReportingPort { get; set; }

            public uint Year { get; set; }
            public ushort Quarter { get; set; }
            public uint DataSourceId { get; set; }
            public List<String> Agents { get; set; }
            public string LastUpdatedBy { get; set; }
            public uint Source { get; set; }
            public DateTime? CreatedDate { get;  set; }

      
    }
}
