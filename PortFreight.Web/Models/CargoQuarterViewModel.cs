using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Models
{
    public class CargoQuarterViewModel
    {
        public string ReportingPort { get; set; }
        public uint Year { get; set; }
        public ushort Quarter { get; set; }
    }
}
