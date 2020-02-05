using System;
using System.Collections.Generic;

namespace PortFreight.Data.Entities
{
    public partial class OrgList
    {
        public int OrgPkId { get; set; }
        public string OrgId { get; set; }
        public string OrgName { get; set; }
        public bool IsAgent { get; set; }
        public bool IsLine { get; set; }
        public bool IsPort { get; set; }
        public bool SubmitsMsd1 { get; set; }
        public bool SubmitsMsd2 { get; set; }
        public bool SubmitsMsd3 { get; set; }
        public bool SubmitsMsd4 { get; set; }
        public bool SubmitsMsd5 { get; set; }
    }
}
