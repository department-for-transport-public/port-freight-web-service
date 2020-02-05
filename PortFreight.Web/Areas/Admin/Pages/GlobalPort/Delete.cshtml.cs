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

namespace PortFreight.Web.Areas.Admin.Pages.GlobalPort
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly PortFreightContext _context;

        public DeleteModel(PortFreightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Data.Entities.GlobalPort GlobalPort { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                ModelState.AddModelError("CustomError", "There has been an error please try again");
                return Page();
            }

            GlobalPort = await _context.GlobalPort.FirstOrDefaultAsync(x => x.Locode == id);

            if (GlobalPort == null)
            {
                ModelState.AddModelError("CustomError", "There has been an error please try again");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                ModelState.AddModelError("CustomError", "There has been an error please try again");
            }

            try
            {
                GlobalPort = await _context.GlobalPort.FindAsync(id);

                if (GlobalPort != null)
                {
                    _context.GlobalPort.Remove(GlobalPort);
                    await _context.SaveChangesAsync();

                    return RedirectToPage("./Index", new { message = "Port deleted" });
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", e.InnerException.Message);
            }
            return Page();
        }
    }
}
