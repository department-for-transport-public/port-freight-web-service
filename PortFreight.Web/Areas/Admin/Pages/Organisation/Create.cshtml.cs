using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;

namespace PortFreight.Web.Areas.Admin.Pages.Organisation
{
    public class CreateModel : PageModel
    {
        private IOrganisation _organisation;

        public CreateModel(IOrganisation organisation)
        {
            _organisation = organisation;
        }

        [BindProperty]
        public OrganisationViewModel Organisation { get; set; }
            
        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
          if (Organisation.OrgName.Trim().Length < 3){
                ModelState.AddModelError("CustomError","OrgName must be between 3 and 80 characters long");
                return Page();
          }
            var methodResult = _organisation.Create(Organisation);
            if (methodResult.SuccessFaliure == Enums.MethodResultOutcome.Failure)
            {
                ModelState.AddModelError("CustomError", methodResult.Message);
                return Page();
            }
            
            return RedirectToPage("./Index", new { message = string.Format("Organisation {0} created successfully", Organisation.OrgId) });
        }
    }
}