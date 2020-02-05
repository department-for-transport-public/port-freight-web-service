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

namespace PortFreight.Web.Areas.Admin.Pages.WorldFleet
{
    public class DeleteModel : PageModel
    {
        private readonly PortFreight.Data.PortFreightContext _context;

        public DeleteModel(PortFreight.Data.PortFreightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Data.Entities.WorldFleet WorldFleet { get; set; }

        public async Task<IActionResult> OnGetAsync(uint? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("CustomError", "There has been an error please try again");
                return Page();
            }

            WorldFleet = await _context.WorldFleet.FirstOrDefaultAsync(m => m.Imo == id);

            if (WorldFleet == null)
            {
                ModelState.AddModelError("CustomError", "There has been an error please try again");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(uint? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("CustomError", "There has been an error please try again");
            }

            try
            {
                WorldFleet = await _context.WorldFleet.FindAsync(id);

                if (WorldFleet != null)
                {
                    _context.WorldFleet.Remove(WorldFleet);
                    await _context.SaveChangesAsync();
                }

                return RedirectToPage("./Index", new { message = "Vessel deleted" });
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", e.InnerException.Message);
            }

            return Page();
        }
    }
}
