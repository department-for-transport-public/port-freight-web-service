using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Web.Areas.Admin.Pages.GlobalPort
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly PortFreightContext _context;

        public EditModel(PortFreightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Data.Entities.GlobalPort GlobalPort { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                ModelState.AddModelError("CustomError", "No record has been found");
                return Page();
            }

            GlobalPort = await _context.GlobalPort.FirstOrDefaultAsync(m => m.Locode == id);

            if (GlobalPort == null)
            {
                ModelState.AddModelError("CustomError", "No record has been found");
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(GlobalPort).State = EntityState.Modified;

            try
            {
                if (GlobalPort.StatisticalPort == null && GlobalPort.CountryCode.ToUpper() == "GB")
                {
                    GlobalPort.StatisticalPort = GlobalPort.Locode;
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GlobalPortExists(GlobalPort.Locode))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index", new { message = "Edit successful" });
        }

        private bool GlobalPortExists(string id)
        {
            return _context.GlobalPort.Any(e => e.Locode == id);
        }
    }
}
