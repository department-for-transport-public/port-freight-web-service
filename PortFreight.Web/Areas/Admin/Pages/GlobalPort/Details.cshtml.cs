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
    public class DetailsModel : PageModel
    {
        private readonly PortFreightContext _context;

        public DetailsModel(PortFreightContext context)
        {
            _context = context;
        }

        public Data.Entities.GlobalPort GlobalPort { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GlobalPort = await _context.GlobalPort.FirstOrDefaultAsync(x => x.Locode == id);

            if (GlobalPort == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
