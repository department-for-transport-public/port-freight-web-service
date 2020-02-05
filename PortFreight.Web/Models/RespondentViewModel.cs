using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Models
{
    public class RespondentViewModel
    {
        public RespondentViewModel() { }

        public RespondentViewModel(RespondentViewModel _respondentVM)
        {
            SenderId = _respondentVM.SenderId;
            PortList = new List<SenderPort>(_respondentVM.PortList);
        }

        public string SenderId { get; set; }
        public List<SenderPort> PortList { get; set; } = new List<SenderPort>();
    }
}
