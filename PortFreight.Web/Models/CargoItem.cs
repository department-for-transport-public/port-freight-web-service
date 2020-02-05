using PortFreight.Web.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortFreight.Web.Models
{
    public class CargoItem
    {
        public Guid Id { get; set; }
        public string Category { get; set; }
        public string Group { get; set; }
        public uint? UnitsWithCargo { get; set; }
        public uint? UnitsWithoutCargo { get; set; }
        public uint? TotalUnits { get; set; }

        [RegularExpression(@"^[0-9]\d{0,9}(\.\d{1,3})?%?$", ErrorMessage = "Limit to 3 decimal places")]
        public double? GrossWeight { get; set; }
        public string Description { get; set; }
    }
}
