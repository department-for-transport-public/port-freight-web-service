using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PortFreight.Data.Entities
{
    public class SenderType
    {
        [Key]
        public string SenderId { get; set; }

        [Display(Name ="Agent")]
        public bool IsAgent { get; set; }

        [Display(Name = "Line")]
        public bool IsLine { get; set; }

        [Display(Name = "Port")]
        public bool IsPort { get; set; }

        public ICollection<SenderIdPort> SenderIdPort { get; set; }
    }
}
