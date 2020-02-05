using System;
using System.Collections.Generic;

namespace PortFreight.Data.Entities
{
    public partial class CargoGroup
    {
        public byte GroupCode { get; set; }
        public string Description { get; set; }
        public bool IsUnitised { get; set; }
        public string HelpText { get; set; }

        public ICollection<CargoCategory> CargoCategory { get; set; }
    }
}
