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

namespace PortFreight.Web.Areas.Admin.Pages.MSD2
{
    public class DetailsModel : PageModel
    {
        private readonly PortFreight.Data.PortFreightContext _context;

        public DetailsModel(PortFreight.Data.PortFreightContext context)
        {
            _context = context;
        }

        public Msd2 Msd2 { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Msd2 = await _context.Msd2.FirstOrDefaultAsync(m => m.Id == id);

            if (Msd2 == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
