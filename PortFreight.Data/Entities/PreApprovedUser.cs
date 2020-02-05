using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PortFreight.Data.Entities
{
    public class PreApprovedUser
    {   [Key]
        public int Id { get; set; }
        [Required]
        public string SenderId { get; set; }
        
        public string EmailAddress { get; set; }
    }
}
