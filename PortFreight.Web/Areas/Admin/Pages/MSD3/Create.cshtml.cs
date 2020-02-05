using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Web.Areas.Admin.Pages.MSD3
{
    public class CreateModel : PageModel
    {
        private readonly PortFreight.Data.PortFreightContext _context;

        public CreateModel(PortFreight.Data.PortFreightContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Msd3 Msd3 { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Msd3.Add(Msd3);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}