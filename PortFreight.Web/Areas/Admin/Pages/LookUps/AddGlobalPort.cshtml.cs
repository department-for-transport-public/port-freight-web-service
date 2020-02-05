using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Web.Areas.Admin.Pages.LookUps
{
    [Authorize(Roles = "Admin")]
    public class AddGlobalPortModel : PageModel
    {
        [BindProperty]
        public Data.Entities.GlobalPort GlobalPort {get; set; } = new Data.Entities.GlobalPort();
        public bool Saved;
        private readonly ILogger<AddGlobalPortModel> _logger;
        private readonly PortFreightContext _context;

        public AddGlobalPortModel(ILogger<AddGlobalPortModel> logger, PortFreightContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.GlobalPort.Add(GlobalPort);
                    _context.SaveChanges();

                    Saved = true;

                    ModelState.Clear();
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("CustomError",
                        "Error writing to database, check all fields and try again");
                    _logger.LogError(e.Message);
                }
            }

            return Page();
        }
    }
}
