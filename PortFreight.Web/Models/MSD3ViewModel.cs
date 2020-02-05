using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Models
{
    public class MSD3ViewModel
    {
        public MSD3ViewModel() { }

        public MSD3ViewModel(MSD3ViewModel _msd3vm)
        {
            ReportingPort = _msd3vm.ReportingPort;
            Year = _msd3vm.Year;
            Quarter = _msd3vm.Quarter;
            
            AgentSummary = new List<Agent>(_msd3vm.AgentSummary);
        }

        public string ReportingPort { get; set; }
        public uint Year { get; set; }
        public ushort Quarter { get; set; }
        public List<Agent> AgentSummary { get; set; } = new List<Agent>();
    }
}
