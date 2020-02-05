
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Data;
using PortFreight.Services.Common;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;

namespace PortFreight.Web.Areas.Admin.Pages.MSD3.MSD3Agents
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public Msd3AgentViewModel Input { get; set; }
        [BindProperty]
        public List<string> Msd3SenderIdList { get; set; }
        public UserManager<PortFreightUser> _userManager { get; }

        private IMsd3AgentDataService _msd3AgentDataService;
        
        public EditModel(IMsd3AgentDataService msd3AgentDataService,
                        UserManager<PortFreightUser> userManager)
        {
            _msd3AgentDataService = msd3AgentDataService;
            _userManager = userManager;
        }
        
        public IActionResult OnGet(int id)
        {
            Input = _msd3AgentDataService.GetMsd3AgentDetail(id);
            if (Input == null) return NotFound();
            Msd3SenderIdList = _msd3AgentDataService.GetShippingLineOrAgentList();
            return Page();
        }

        public IActionResult OnPost()
        {
            var lastUpdatedByUser = _userManager.GetUserName(HttpContext.User);
            var methodResult = _msd3AgentDataService.Update(Input, lastUpdatedByUser);
            if (methodResult.SuccessFaliure == Enums.MethodResultOutcome.Failure)
            {
                ModelState.AddModelError("CustomError", methodResult.Message);
                Msd3SenderIdList = _msd3AgentDataService.GetShippingLineOrAgentList();
                return Page();
            }
            return RedirectToPage("./Index", new { id = Input.Msd3Id, message = "Shipping Line / Agent updated successfully" });

        }

       
    }
}
