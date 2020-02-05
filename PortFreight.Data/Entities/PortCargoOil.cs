using System;
using System.Collections.Generic;

namespace PortFreight.Data.Entities
{
    public partial class PortCargoOil
    {
        public string Locode { get; set; }
        public bool AllowCategory12 { get; set; }
        public bool AllowCategory13Outward { get; set; }
    }
}
