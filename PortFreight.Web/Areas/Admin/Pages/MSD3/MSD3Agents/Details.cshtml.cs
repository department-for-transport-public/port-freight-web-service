using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;

namespace PortFreight.Web.Areas.Admin.Pages.MSD3.MSD3Agents
{
    public class DetailsModel : PageModel
    {
        private IMsd3AgentDataService _msd3AgentDataService;

        public DetailsModel(IMsd3AgentDataService msd3AgentDataService)
        {
            _msd3AgentDataService = msd3AgentDataService;
        }    

        [BindProperty]
        public Msd3AgentViewModel Msd3agent { get; set; } = new Msd3AgentViewModel();

        public IActionResult OnGet(int id)
        {
            Msd3agent = _msd3AgentDataService.GetMsd3AgentDetail(id);
            if (Msd3agent == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
