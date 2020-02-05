using PortFreight.ViewModel.Validators;
using System.ComponentModel.DataAnnotations;

namespace PortFreight.ViewModel
{
    public class OrganisationViewModel
    {       
        public int OrgPkId { get; set; }
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "{0} must be {1} characters long" )]
        [SpaceNotAllowed]
        public string OrgId { get; set; }
        [Required]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "{0} must be between {2} and {1} characters long")]
        public string OrgName { get; set; }
        public bool IsAgent { get; set; }
        public bool IsLine { get; set; }
        public bool IsPort { get; set; }
        public bool submits_msd1 { get; set; }
        public bool submits_msd2 { get; set; }
        public bool submits_msd3 { get; set; }
        public bool submits_msd4 { get; set; }
        public bool submits_msd5 { get; set; }
    }
}
