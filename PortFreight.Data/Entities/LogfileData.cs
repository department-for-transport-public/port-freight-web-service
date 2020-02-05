using System;
using System.Collections.Generic;

namespace PortFreight.Data.Entities
{
    public partial class LogFileData
    {
        public int Id { get; set; }
        public int FileRefId { get; set; }
        public DateTime DateTime { get; set; }
        public string Description { get; set; }
        public bool IsEmailed { get; set; } = false;
    }
}
