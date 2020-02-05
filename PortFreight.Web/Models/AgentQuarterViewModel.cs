using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Models
{
    public class AgentQuarterViewModel
    {
        public string ReportingPort { get; set; }
        public uint Year { get; set; }
        public ushort Quarter { get; set; }
    }
}
