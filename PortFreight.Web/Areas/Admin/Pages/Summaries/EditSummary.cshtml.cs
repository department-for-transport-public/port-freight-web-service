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
using PortFreight.Services.Common;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Areas.Admin.Pages
{
    public class EditSummaryModel : PageModel
    {
        [BindProperty]
        [Required]
        public byte CategoryCode { get; set; }
        [BindProperty]
        public uint? UnitsWithCargo { get; set; }
        [BindProperty]
        public uint? UnitsWithoutCargo { get; set; }
        [BindProperty]
        public uint? TotalUnits { get; set; }
        [BindProperty]
        public decimal? GrossWeight { get; set; }
        [BindProperty]
        [Required]
        public string Description { get; set; }
        
        public Msd1CargoSummary CargoSummary;
        private readonly IHelperService _helperService;
        private Msd1Data Msd;
        private readonly PortFreightContext _context;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly ILogger<EditSummaryModel> _logger;

        public EditSummaryModel(PortFreightContext context,
            UserManager<PortFreightUser> userManager,
            ILogger<EditSummaryModel> logger,
            IHelperService helperService,
            UserDbContext userContext)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _helperService = helperService;
        }

        public IActionResult OnGet()
        {
            try
            {
                CargoSummary = _context.Msd1CargoSummary.Where(x => x.Id == int.Parse(TempData.Peek("SumId").ToString()))
                    .First();

                Msd = _context.Msd1Data.Where(x => x.Msd1Id == TempData.Peek("Msd1Id").ToString()).First();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return RedirectToPage("/Index", new { area = "Admin" });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                CargoSummary = _context.Msd1CargoSummary.Where(x => x.Id == int.Parse(TempData.Peek("SumId").ToString()))
                .First();

                CargoSummary.CategoryCode = CategoryCode;
                CargoSummary.UnitsWithCargo = UnitsWithCargo;
                CargoSummary.UnitsWithoutCargo = UnitsWithoutCargo;
                CargoSummary.TotalUnits = TotalUnits;
                CargoSummary.GrossWeight = GrossWeight;
                CargoSummary.Description = Description;

                await UpdateLastUpdatedAsync();

                var totalWeightAllowed = _helperService.GetDeadweightByIMO(Msd.Imo) * Msd.NumVoyages;

                if (CargoSummary.GrossWeight > totalWeightAllowed)
                {
                    ModelState.AddModelError("GrossWeight", "The Gross Weight is larger than this ship can carry");
                    return Page();
                }

                if (!DoesCategoryCodeExist(CargoSummary.CategoryCode))
                {
                    ModelState.AddModelError("CategoryCode", $"The Category Code {CargoSummary.CategoryCode} is not valid");
                    return Page();
                }

                _context.SaveChanges();

                TempData.Put("EditSuccess", "Cargo summary has been successfully amended");

                TempData["Msd1Id"] = CargoSummary.Msd1Id;

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return RedirectToPage("/Summaries/ViewSummary", new { area = "Admin" });
            }

            return RedirectToPage("/Summaries/ViewSummary", new { area = "Admin" });
        }

        private async Task UpdateLastUpdatedAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            Msd = _context.Msd1Data.Where(x => x.Msd1Id == TempData.Peek("Msd1Id").ToString()).First();
            Msd.ModifiedDate = DateTime.Now;
            Msd.LastUpdatedBy = user.UserName;
        }

        private bool DoesCategoryCodeExist(int cargoCategory)
        {
            return _context.CargoCategory.Any(e => e.CategoryCode == cargoCategory);

        }


    }
}
