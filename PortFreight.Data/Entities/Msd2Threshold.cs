using System;
using System.Collections.Generic;

namespace PortFreight.Data.Entities
{
    public partial class Msd2Threshold
    {
        public int Id { get; set; }
        public string ThresholdCategory { get; set; }
        public int RangeFrom { get; set; }
        public int RangeTo { get; set; }
        public short Tolerance { get; set; }
    }
}
