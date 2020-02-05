using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;

namespace PortFreight.Web.Areas.Admin.Pages.Organisation
{
    public class DetailsModel : PageModel
    {
        private IOrganisation _organisation;
        public OrganisationViewModel Organisation { get; set; }

        public DetailsModel(IOrganisation organisation)
        {
            _organisation = organisation;
        }
       
        public IActionResult OnGet(int id)
        {
            Organisation = _organisation.GetOrganisationDetail(id);
            return Page();
        }
    }
}
