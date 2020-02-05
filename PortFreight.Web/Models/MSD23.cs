using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Models
{
    public class MSD23
    {
        public MSD23() { }

        public MSD23(MSD23 _msd23)
        {
            Year = _msd23.Year;
            Quarter = _msd23.Quarter;
            Port = _msd23.Port;
            PortName = _msd23.PortName;
        }
        public uint Year { get; set; }
        public ushort Quarter { get; set; }
        public string Port { get; set; }
        public string PortName { get; set; }
    }
}
