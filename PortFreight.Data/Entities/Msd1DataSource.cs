using System;
using System.Collections.Generic;

namespace PortFreight.Data.Entities
{
    public partial class Msd1DataSource
    {
        public Msd1DataSource()
        {
            Msd1Data = new HashSet<Msd1Data>();
        }

        public uint DataSourceId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        public ICollection<Msd1Data> Msd1Data { get; set; }
    }
}
