using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Models
{
    public class SenderPort
    {
        public Guid Id { get; set; }
        public string SenderId { get; set; }
        public string PortName { get; set; }
        public string Locode { get; set; }
    }
}
