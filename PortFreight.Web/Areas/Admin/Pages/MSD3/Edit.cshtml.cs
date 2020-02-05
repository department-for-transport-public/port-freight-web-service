using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Web.Areas.Admin.Pages.MSD3
{
    public class EditModel : PageModel
    {
        private readonly PortFreight.Data.PortFreightContext _context;
        private readonly UserManager<PortFreightUser> _userManager;

        public EditModel(PortFreight.Data.PortFreightContext context, UserManager<PortFreightUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Msd3 Msd3 { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Msd3 = await _context.Msd3.FirstOrDefaultAsync(m => m.Id == id);

            if (Msd3 == null)
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

            Msd3.DataSourceId = Msd3.DataSourceId;
            Msd3.ModifiedDate = DateTime.Now;
            Msd3.LastUpdatedBy = _userManager.GetUserName(HttpContext.User);
            _context.Attach(Msd3).State = EntityState.Modified;



            try
            {
                if(!Msd3ReportingPortExists(Msd3.ReportingPort))
                {
                    ModelState.AddModelError("CustomError", "There is a problem with the Reporting Port you have added");
                    return Page();
                }
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Msd3Exists(Msd3.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index", new { message = "Submission succesfully updated"});
        }

        private bool Msd3Exists(string id)
        {
            return _context.Msd3.Any(e => e.Id == id);
        }

        private bool Msd3ReportingPortExists(string reportingPort)
        {
            return _context.Msd3.Any(e => e.ReportingPort == reportingPort);
        }
    }
}
