using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Services.Common;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Areas.Admin.Pages.Organisation
{
    public class EditModel : PageModel
    {
        private IOrganisation _organisation;

        public EditModel(IOrganisation organisation)
        {
            _organisation = organisation;
        }

        [BindProperty]
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
           
            var methodResult = _organisation.Update(Organisation);
            if (methodResult.SuccessFaliure == Enums.MethodResultOutcome.Failure)
            {
                ModelState.AddModelError("CustomError", methodResult.Message);
                return Page();
            }           

           return RedirectToPage("./Index", new { message = string.Format("Organisation {0} updated successfully",Organisation.OrgId) });
        }

    }
}
