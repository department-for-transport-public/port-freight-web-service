using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Web.Areas.Admin.Pages.MSD2
{
    public class CreateModel : PageModel
    {
        private readonly PortFreightContext _context;
        private readonly UserManager<PortFreightUser> _userManager;

        public CreateModel(PortFreightContext context, UserManager<PortFreightUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Msd2 Msd2 { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Msd2.CreatedDate = DateTime.Now;
            Msd2.CreatedBy = _userManager.GetUserName(HttpContext.User);

            _context.Msd2.Add(Msd2);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}