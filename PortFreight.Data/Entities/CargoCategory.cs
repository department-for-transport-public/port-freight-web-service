using System;
using System.Collections.Generic;

namespace PortFreight.Data.Entities
{
    public enum PortCategory : int
    {
        CrudeOilCode = 12,
        OilProductsCode = 13
    }
    public partial class CargoCategory
    {
        public byte CategoryCode { get; set; }
        public string Description { get; set; }
        public string HelpText { get; set; }
        public bool TakesCargo { get; set; }
        public bool HasWeight { get; set; }
        public byte? MaxWeight { get; set; }
        public byte GroupCode { get; set; }

        public CargoGroup GroupCodeNavigation { get; set; }
    }
}
