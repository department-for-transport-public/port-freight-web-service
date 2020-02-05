using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PortFreight.Data.Entities
{
    public partial class GlobalPort
    {
        [Required]
        [StringLength(5, MinimumLength = 5)]
        public string Locode { get; set; }
        [Required]
        public string PortName { get; set; }
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string CountryCode { get; set; }
        [StringLength(5, MinimumLength = 5)]
        public string StatisticalPort { get; set; }
        public bool ForMsd1LoadUnload { get; set; } = false;
        public bool ForMsd1ReportingPort { get; set; } = false;
        public bool ForMsd2 { get; set; } = false;
        public bool ForMsd3 { get; set; } = false;
        public bool ForMsd4 { get; set; } = false;
        public bool ForMsd5 { get; set; } = false;
        public ICollection<SenderIdPort> SenderIdPorts { get; set; }
    }
}
