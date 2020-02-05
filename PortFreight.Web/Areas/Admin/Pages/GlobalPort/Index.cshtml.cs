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

namespace PortFreight.Web.Areas.Admin.Pages.GlobalPort
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly PortFreightContext _context;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        [Required(ErrorMessage = "Enter either a Locode or Port name")]
        public string SearchInput { get; set; }

        public IList<Data.Entities.GlobalPort> GlobalPort { get; set; }


        public IndexModel(PortFreightContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task OnGetAsync(string message)
        {
            if (message != null && message != "")
            {
                ModelState.AddModelError("SuccessMessage", message);
            }

            GlobalPort = await _context.GlobalPort.Take(10).ToListAsync();


        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            try
            {

                if (_context.GlobalPort.Any(x => x.Locode.Contains(SearchInput)))
                {
                    await GetPortsByLocodeAsync();
                }

                if (_context.GlobalPort.Any(x => x.PortName.Contains(SearchInput)))
                {
                    if (GlobalPort != null && GlobalPort.Count > 0)
                    {
                        foreach (Data.Entities.GlobalPort port in GetGlobalPortsByName())
                        {
                            GlobalPort.Add(port);
                        }
                    }
                    else
                    {
                        GlobalPort = GetGlobalPortsByName();
                    }
                }

                if (GlobalPort.Count != 0)
                {
                    GlobalPort = GlobalPort.Distinct()
                        .OrderBy(x => x.PortName)
                        .ToList();
                }
                else
                {
                    ModelState.AddModelError("CustomError", "No Ports have been found");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                ModelState.AddModelError("CustomError", 
                    "There has been an error connecting to the database. Please check the inputs and try again.");
            }

            return Page();
        }

        private IList<Data.Entities.GlobalPort> GetGlobalPortsByName()
        {
            return _context.GlobalPort.Where(x => x.PortName.Contains(SearchInput))
                                        .OrderBy(x => x.PortName).ToList();
        }

        private async Task<IList<Data.Entities.GlobalPort>> GetPortsByLocodeAsync()
        {
            return GlobalPort = await _context.GlobalPort.Where(x => x.Locode.Contains(SearchInput))
                                        .ToListAsync();
        }
    }
}
