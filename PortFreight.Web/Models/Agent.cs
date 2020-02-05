using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Models
{
    public class Agent
    {
        public Guid Id { get; set; }
        public string ShippingAgent { get; set; }
    }
}