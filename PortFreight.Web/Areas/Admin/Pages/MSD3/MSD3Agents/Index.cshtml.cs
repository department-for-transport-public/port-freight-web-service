using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;

namespace PortFreight.Web.Areas.Admin.Pages.MSD3.MSD3Agents
{
    public class IndexModel : PageModel
    {
        private IMsd3AgentDataService _msd3AgentDataService;

        public IndexModel(IMsd3AgentDataService msd3AgentDataService)
        {
            _msd3AgentDataService = msd3AgentDataService;
        }

        public List<Msd3AgentViewModel> Msd3agents { get;set; }

        public string Msd3Id { get; set; }



        public IActionResult OnGet(string id, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("SuccessMessage", message);
            }
            Msd3agents = _msd3AgentDataService.GetAgentListFilteredByMsd3Id(id);
            Msd3Id = id;
            return Page();
        }
    }
}
