using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Web.Areas.Admin.Pages.MSD3
{
    public class DetailsModel : PageModel
    {
        private readonly PortFreight.Data.PortFreightContext _context;

        public DetailsModel(PortFreight.Data.PortFreightContext context)
        {
            _context = context;
        }

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
    }
}
