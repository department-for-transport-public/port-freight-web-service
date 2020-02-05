using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Areas.Admin.Pages
{
    public class ViewSummaryModel : PageModel
    {
        private readonly PortFreightContext _context;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly ILogger<ViewSummaryModel> _logger;

        public List<Msd1CargoSummary> CargoSummaries;
        public string SuccessMessage;
        public ViewSummaryModel(PortFreightContext context,
            UserManager<PortFreightUser> userManager,
            ILogger<ViewSummaryModel> logger,
            UserDbContext userContext)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            try
            {
                SuccessMessage = TempData.Get<string>("EditSuccess") ?? string.Empty;

                CargoSummaries = _context.Msd1CargoSummary.Where(x => x.Msd1Id == TempData.Peek("Msd1Id").ToString())
                    .ToList();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", "There has been an error connecting to the database");
                _logger.LogError(e.Message);
            }

            return Page();
        }

        public IActionResult OnPostEdit(int SumId)
        {
            TempData["SumId"] = SumId.ToString();

            return RedirectToPage("/Summaries/EditSummary", new { area = "Admin" });
        }
    }
}