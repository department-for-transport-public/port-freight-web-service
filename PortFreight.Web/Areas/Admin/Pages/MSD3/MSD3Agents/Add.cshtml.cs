
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
    public class AddModel : PageModel
    {
        private IMsd3AgentDataService _msd3AgentDataService;
        [BindProperty]
        public Msd3AgentViewModel Input { get; set; } = new Msd3AgentViewModel();
        [BindProperty]
        public List<string> Msd3SenderIdList { get; set; }
        public UserManager<PortFreightUser> _userManager { get; }

        public AddModel(
        IMsd3AgentDataService msd3AgentDataService,
         UserManager<PortFreightUser> userManager)
        {
          
            _msd3AgentDataService = msd3AgentDataService;
            _userManager = userManager;
        }

        public IActionResult OnGet(string msd3Id)
        {
            Msd3SenderIdList = _msd3AgentDataService.GetShippingLineOrAgentList();
            Input.Msd3Id = msd3Id;
            return Page();
        }               

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {               
                return Page();
            }
            string lastUpdatedByUser = _userManager.GetUserName(HttpContext.User);
            var methodResult = _msd3AgentDataService.UpdateLastUpdatedBy(Input, lastUpdatedByUser);

            if (methodResult.SuccessFaliure == Enums.MethodResultOutcome.Failure)
            {
                ModelState.AddModelError("CustomError", methodResult.Message);
                return Page();
            }
            return RedirectToPage("./Index", new { id = Input.Msd3Id , message = string.Format("Shipping Line/Agent {0} added successfully", Input.SenderId)});
        }
    }
}