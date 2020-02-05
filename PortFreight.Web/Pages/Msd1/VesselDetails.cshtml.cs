using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;
using PortFreight.Services.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Pages.Msd1
{
    public class VesselDetailsModel : BaseMsd1PageModel
    {
        public MSD1 MSD1 { get; set; }
        private readonly PortFreightContext _context;
        private readonly ILogger<VesselDetailsModel> _logger;
        private readonly IHelperService _helperService;
        public List<string> Vessels;
        [BindProperty]
        public bool FromSummary { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Enter a valid IMO number or Vessel Name")]
        public string Vessel { get; set; }

        public VesselDetailsModel(
            PortFreightContext context,
            ILogger<VesselDetailsModel> logger,
            IHelperService helperService)
        {
            _context = context;
            _logger = logger;
            _helperService = helperService;
        }

        private void InitialisePage()
        {
            Vessels = _context.WorldFleet.AsNoTracking().Select
             (
                 x =>
                 x.Imo.ToString() +
                 " - " + x.ShipName.ToString() +
                 " (" + x.FlagCode.ToString() + ")"
             )
             .ToList();

            if (CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD1Key] != null)
            {
                MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));

                if (MSD1.Imo != 0)
                {
                    Vessel = MSD1.Imo.ToString() +
                     " - " + MSD1.ShipName.ToString() +
                     " (" + MSD1.FlagCode + ")";
                }
            }

            FromSummary = Helpers.ReturnBoolFromQueryString(HttpContext, "FromSummary");
        }
        public IActionResult OnGet()
        {
            InitialisePage();

            return Page();
        }
        public IActionResult OnPost(string vesselDetails)
        {

            Vessel = vesselDetails;
        
            if(Vessel != null)
            {
            ModelState.Clear();
            TryValidateModel(Vessel);
            }

            if (!ModelState.IsValid)
            {
                InitialisePage();

                return Page();
            }

            try
            {
                uint imo = Convert.ToUInt32(Vessel.Split('-').FirstOrDefault().TrimEnd());
                var vessel = _context.WorldFleet
                    .Where(a => a.Imo == imo)
                    .First();

                MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key))
                {
                    Imo = vessel.Imo,
                    Deadweight = vessel.Deadweight,
                    ShipName = vessel.ShipName,
                    FlagCode = vessel.FlagCode
                };
                TempData.Put(MSD1Key, MSD1);

                return RedirectToPage("./VoyageDetails", new { FromSummary = FromSummary.ToString(), IsEdited = FromSummary.ToString() });
            }

            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ModelState.AddModelError("CustomError", "Enter a valid IMO number or Vessel Name");
                InitialisePage();
                return Page();
            }
        }
    }
}
