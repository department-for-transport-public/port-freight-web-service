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
    public class DetailsModel : PageModel
    {
        private readonly PortFreight.Data.PortFreightContext _context;

        public DetailsModel(PortFreight.Data.PortFreightContext context)
        {
            _context = context;
        }

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
    }
}
