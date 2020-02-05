using System;
using System.Collections.Generic;
using System.Text;

namespace PortFreight.Services.Models
{
    public class CargoItem
    {
        public Guid Id { get; set; }
        public string Category { get; set; }
        public string Group { get; set; }
        public uint? UnitsWithCargo { get; set; }
        public uint? UnitsWithoutCargo { get; set; }
        public uint? TotalUnits { get; set; }
        public double? GrossWeight { get; set; }
        public string Description { get; set; }
    }
}
