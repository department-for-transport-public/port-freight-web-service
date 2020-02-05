using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;

namespace PortFreight.Web.Areas.Admin.Pages.OrgList
{
    public class DeleteModel : PageModel
    {
        private IOrganisation _organisation;

        public DeleteModel(IOrganisation organisation)
        {
            _organisation = organisation;
        }
        public OrganisationViewModel Organisation { get; set; }

        public IActionResult OnGet(int id)
        {
            Organisation = _organisation.GetOrganisationDetail(id);

            if (Organisation == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPost(int id)
        {
            Organisation = _organisation.GetOrganisationDetail(id);
            _organisation.Delete(id);           
            return RedirectToPage("./Index", new { message = string.Format("Organisation {0} deleted", Organisation.OrgName) });
        }
    }
}
