using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Data;
using PortFreight.Services.Common;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;

namespace PortFreight.Web.Areas.Admin.Pages.MSD3.MSD3Agents
{
    public class DeleteModel : PageModel
    {
        private IMsd3AgentDataService _msd3AgentDataService;
         public UserManager<PortFreightUser> _userManager { get; }
        [BindProperty]
        public Msd3AgentViewModel Input { get; set; } = new Msd3AgentViewModel();

        public DeleteModel(IMsd3AgentDataService msd3AgentDataService,
        UserManager<PortFreightUser> userManager)
        {
            _msd3AgentDataService = msd3AgentDataService;
            _userManager = userManager;
        }

        [BindProperty]
        public Msd3AgentViewModel Msd3agent { get; set; } = new Msd3AgentViewModel();
        
        public IActionResult OnGet(int id)
        {
            Msd3agent = _msd3AgentDataService.GetMsd3AgentDetail(id);
            Input.Msd3Id = id.ToString();

            if (Msd3agent == null)
            {
                return NotFound();
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            string lastUpdatedByUser = _userManager.GetUserName(HttpContext.User);
            var methodResult = _msd3AgentDataService.Delete(Msd3agent, lastUpdatedByUser); 
            if (methodResult.SuccessFaliure == Enums.MethodResultOutcome.Failure)
            {
                ModelState.AddModelError("CustomError", methodResult.Message);
                return Page();
            }
            
            return RedirectToPage("./Index", new { id = Msd3agent.Msd3Id  , message = string.Format("Shipping Line/Agent {0} deleted", Msd3agent.SenderId) });
        }
    }
}
