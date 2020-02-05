using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PortFreight.Data.Entities
{
    public class SenderIdPort
    {
        public string SenderId { get; set; }

        public SenderType SenderType { get; set; }

        [NotMapped]
        public List<string> Locodes { get; set; } = new List<string>();

        public string Locode { get; set; }

        public GlobalPort GlobalPort { get; set; }
    }
}
