using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Web.Utilities;
using PortFreight.Services.MSD2;

namespace PortFreight.Web.Areas.Admin.Pages.MSD2
{
    public class EditModel : PageModel
    {
        private readonly PortFreightContext _context;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly ILogger<EditModel> _logger;
        private readonly IMsd2DataService _msd2DataService;
        [BindProperty]
        public bool CorrectWeight { get; set; }
        public Data.Entities.Msd2 msd2;
        public bool showCheckbox { get; set; }
        [BindProperty]
        public string ReportingPort { get; set; }
        [BindProperty]
        public string SenderId { get; set; }
        [BindProperty]
        public int Id { get; set; }
        [BindProperty]
        public DateTime CreatedDate { get; set; }
        [BindProperty]
        public uint Year { get; set; }
        [BindProperty]
        public ushort Quarter { get; set; }
        [BindProperty]
        public decimal GrossWeightInward { get; set; }
        [BindProperty]
        public uint TotalUnitsInward { get; set; }
        [BindProperty]
        public decimal GrossWeightOutward { get; set; }
        [BindProperty]
        public uint TotalUnitsOutward { get; set; }
        [BindProperty]
        public uint PassengerVehiclesOutward { get; set; }
        [BindProperty]
        public uint PassengerVehiclesInward { get; set; }


        public EditModel(PortFreightContext context,
         UserManager<PortFreightUser> userManager,
         IMsd2DataService msd2DataService,
         ILogger<EditModel> logger,

         UserDbContext userContext)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _msd2DataService = msd2DataService;
        }


        public IActionResult OnGet()
        {
            try
            {
                msd2 = TempData.GetKeep<Msd2>("Msd2");
            }
            catch
            {
                return Page();
            }
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            PortFreightUser user = await _userManager.GetUserAsync(HttpContext.User);
            mapInputsToMSD2(user);
            if (!WeightIsValid(msd2.GrossWeightInward, msd2.GrossWeightOutward, msd2.Year, msd2.Quarter, msd2.ReportingPort, msd2.SenderId))
            {
                return Page();
            }

            if (!Msd2ReportingPortExists(msd2.ReportingPort))
            {
                ModelState.AddModelError("ReportingPort", "The Reporting port entered is not in the global port list");
                return Page();
            }

            try
            {
                _context.Msd2.Update(msd2);
                _context.SaveChanges();

                TempData.Put("EditSuccess", "Submission has been successfully amended");

                return RedirectToPage("./Index", new { area = "Admin" });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);

                ModelState.AddModelError("CustomError", "There is a problem save the changes");

                return Page();
            }

        }

        public void mapInputsToMSD2(PortFreightUser user)
        {
            msd2 = TempData.GetKeep<Msd2>("Msd2");

            msd2.Id = Id;
            msd2.SenderId = SenderId;
            msd2.CreatedDate = CreatedDate;
            msd2.ReportingPort = ReportingPort;
            msd2.Year = Year;
            msd2.Quarter = Quarter;
            msd2.GrossWeightInward = GrossWeightInward;
            msd2.TotalUnitsInward = TotalUnitsInward;
            msd2.PassengerVehiclesInward = PassengerVehiclesInward;
            msd2.GrossWeightOutward = GrossWeightOutward;
            msd2.TotalUnitsOutward = TotalUnitsOutward;
            msd2.PassengerVehiclesOutward = PassengerVehiclesOutward;
            msd2.LastUpdatedBy = user.Email;

        }

        private bool Msd2ReportingPortExists(string reportingPort)
        {
            return _context.Msd2.Any(e => e.ReportingPort == reportingPort);
        }

        private bool WeightIsValid(decimal grossWeightIn, decimal grossWeightOut, uint currentYear, ushort currentQuarter, string port, string senderId)
        {
            if (CorrectWeight)
            {
                return true;
            }

            var (previousYearGWTInward, thresoldInward, invalidWeightIn) = _msd2DataService.ValidateGrossWeightInwards(grossWeightIn, currentYear, currentQuarter, port, senderId);
            if (invalidWeightIn)
            {
                ModelState.AddModelError("GrossWeightInward", $"The previously entered weight for this quater last year was {previousYearGWTInward}. The amount you entered is outside the threshold of: {thresoldInward}. If this is correct, check the box at the bottom of the page.");
                showCheckbox = true;
            }

            var (previousYearGWTOutward, thresoldOutward, invalidWeightOut) = _msd2DataService.ValidateGrossWeightOutwards(grossWeightOut, currentYear, currentQuarter, port, senderId);
            if (invalidWeightOut)
            {
                ModelState.AddModelError("GrossWeightOutward", $"The previously entered weight for this quater last year was {previousYearGWTOutward}. The amount you entered is outside the threshold of: {thresoldOutward}. If this is correct, check the box at the bottom of the page");
                showCheckbox = true;
            }

            return !invalidWeightIn && !invalidWeightOut;
        }
    }

}
