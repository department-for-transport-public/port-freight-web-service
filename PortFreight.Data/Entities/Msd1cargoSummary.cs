using System;
using System.Collections.Generic;

namespace PortFreight.Data.Entities
{
    public partial class Msd1CargoSummary
    {
        public int Id { get; set; }
        public string Msd1Id { get; set; }
        public byte CategoryCode { get; set; }
        public uint? UnitsWithCargo { get; set; }
        public uint? UnitsWithoutCargo { get; set; }
        public uint? TotalUnits { get; set; }
        public decimal? GrossWeight { get; set; }
        public string Description { get; set; }
    }
}
