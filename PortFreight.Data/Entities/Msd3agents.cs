using System;
using System.Collections.Generic;

namespace PortFreight.Data.Entities
{
    public partial class Msd3agents
    {
        public int Id { get; set; }
        public string Msd3Id { get; set; }
        public string SenderId { get; set; }

        public Msd3 Msd3 { get; set; }
    }
}
