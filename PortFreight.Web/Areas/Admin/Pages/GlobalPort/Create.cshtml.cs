using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Web.Areas.Admin.Pages.GlobalPort
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly PortFreightContext _context;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public Data.Entities.GlobalPort GlobalPort { get; set; }

        public CreateModel(PortFreightContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                if (GlobalPort.StatisticalPort == null && GlobalPort.CountryCode.ToUpper() == "GB")
                {
                    GlobalPort.StatisticalPort = GlobalPort.Locode;
                }

                 bool alreadyExists = _context.GlobalPort.Any(x => x.PortName == GlobalPort.PortName );

                    if (alreadyExists)
                    {
                        ModelState.AddModelError("CustomError", "The Port Name already exists");
                    }

                _context.GlobalPort.Add(GlobalPort);
                await _context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                ModelState.AddModelError("CustomError", e.InnerException.Message);

                _logger.LogError(e.Message);

                return Page();
            }
            

            return RedirectToPage("./Index", new { message = "Port added" });
        }
    }
}