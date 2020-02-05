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

namespace PortFreight.Web.Areas.Admin.Pages.WorldFleet
{
    public class EditModel : PageModel
    {
        private readonly PortFreight.Data.PortFreightContext _context;

        public EditModel(PortFreight.Data.PortFreightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Data.Entities.WorldFleet WorldFleet { get; set; }

        public async Task<IActionResult> OnGetAsync(uint? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WorldFleet = await _context.WorldFleet.FirstOrDefaultAsync(m => m.Imo == id);

            if (WorldFleet == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(WorldFleet).State = EntityState.Modified;
         
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorldFleetExists(WorldFleet.Imo))
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

        private bool WorldFleetExists(uint id)
        {
            return _context.WorldFleet.Any(e => e.Imo == id);
        }
    }
}
