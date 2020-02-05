using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using PortFreight.Data;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Areas.Identity.Pages.Account
{
    public class RegisterConfirmationModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }
        public void OnGet()
        {
            var applicationUser = TempData.GetKeep<PortFreightUser>("PreApprovedUser");
            if (applicationUser != null)
            {
                Input = new InputModel();
                Input.Email = applicationUser.Email;
            }
        }

        public class InputModel
        {
            [EmailAddress]
            public string Email { get; set; }
        }
    }
}