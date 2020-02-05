using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Web.Pages.Profile
{
    public class ContactDetailsModel : PageModel
    {
        [BindProperty]
        public ContactDetails ContactDetails { get; set; }

        public string SuccessMessage { get; set; }

        private readonly UserManager<PortFreightUser> _userManager;
        private readonly PortFreightContext _context;

        public ContactDetailsModel(UserManager<PortFreightUser> userManager, PortFreightContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user ==  null)
            {
                return RedirectToPage("/Account/Logout", new { area = "Identity" });
            }

            ContactDetails = _context.ContactDetails
                .Where(x => x.SenderId == user.SenderId)
                .FirstOrDefault() ?? new ContactDetails() { SenderId = user.SenderId };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                if (ContactDetails != null)
                {
                    var result = await _context.ContactDetails
                        .AnyAsync(x => x.SenderId == ContactDetails.SenderId) ? _context.Update(ContactDetails) : _context.Add(ContactDetails);
                    _context.SaveChanges();
                    SuccessMessage = "Contact details successfully saved";
                    return Page();
                }
            }

            return Page();
        }
    }
}