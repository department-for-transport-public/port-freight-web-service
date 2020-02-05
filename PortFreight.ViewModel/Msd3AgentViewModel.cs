
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PortFreight.ViewModel
{
    public class Msd3AgentViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Msd3Id { get; set; }
        [Required]
        [Display (Name ="Line/Agent")]
        public string SenderId { get; set; }
       
    }
}
