using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Web.Areas.Admin.Pages.WorldFleet
{
    public class IndexModel : PageModel
    {
        private readonly PortFreight.Data.PortFreightContext _context;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        [Required(ErrorMessage = "Enter either a ship name, IMO, or call sign")]
        public string SearchInput { get; set; }

        public IndexModel(PortFreightContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IList<Data.Entities.WorldFleet> WorldFleet { get;set; }

        public async Task OnGetAsync(string message)
        {
            if (message != null && message != "")
            {
                ModelState.AddModelError("SuccessMessage", message);
            }

            WorldFleet = await _context.WorldFleet.Take(10).ToListAsync();
        }

        public async Task OnPostSearch()
        {
            try
            {
                                
                if (uint.TryParse(SearchInput, out uint imo))
                {
                    WorldFleet = await _context.WorldFleet.Where(x => x.Imo == imo).ToListAsync();
                }
                else if (_context.WorldFleet.Any(x => x.ShipName == SearchInput))
                {
                    WorldFleet = await _context.WorldFleet.Where(x => x.ShipName == SearchInput)
                        .OrderBy(x => x.ShipName)
                        .ToListAsync();
                }
                else if (_context.WorldFleet.Any(x => x.CallSign == SearchInput))
                {
                    WorldFleet = await _context.WorldFleet.Where(x => x.CallSign == SearchInput).ToListAsync();
                }
                else
                {
                    ModelState.AddModelError("CustomError", "No ships have been found");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                ModelState.AddModelError("CustomError", "There has been an error connecting to the database. Please check the inputs and try again.");
            }
        }
    }
}
