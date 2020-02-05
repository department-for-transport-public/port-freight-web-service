using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;

namespace PortFreight.Web.Areas.Admin.Pages.Organisation
{
    public class IndexModel : PageModel
    {
        private IOrganisationSearch _organisationSearch;

        [BindProperty]
        [Required(ErrorMessage = "Search criteria is required")]
        
        public string SearchInput { get; set; }
        public List<OrganisationViewModel> OrganisationList { get; set; }

        public IndexModel(IOrganisationSearch organisationSearch)
        {
            _organisationSearch = organisationSearch;
        }     
        
        public ActionResult OnGet(string message)
        {
            if (message != null && message != "")
            {
                ModelState.AddModelError("SuccessMessage", message);
            }

            OrganisationList =_organisationSearch.Search(string.Empty);
            return Page();
        }
        
        public ActionResult OnPostSearch()
        {
            try
            {
                OrganisationList = _organisationSearch.Search(SearchInput);

                if (OrganisationList.Count == 0)
                {
                    ModelState.AddModelError("SearchInput", "No results found");
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("SearchInput", "Error occurred while searching, please retry");
            }

            return Page();
        }
    }
}
