using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PortFreight.Data.Entities
{
    public class ContactDetails
    {
        [Key]
        public string SenderId { get; set; }

        [Required(ErrorMessage = "Enter a company name")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Enter a contact name")]
        public string ContactName { get; set; }             

        [Required(ErrorMessage = "Enter a phone number")]
        [Phone(ErrorMessage = "Enter a vaild phone number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Enter a valid address line")]
        public string Addr1 { get; set; }

        [StringLength(100)]
        public string Addr2 { get; set; }

        [StringLength(100)]
        public string Addr3 { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string County { get; set; }

        [Required(ErrorMessage = "Enter a post code")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Enter a vaild post code")]
        public string Postcode { get; set; }
    }
}
