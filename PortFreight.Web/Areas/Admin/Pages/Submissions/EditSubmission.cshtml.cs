using System;
using System.Collections.Generic;
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
    public class EditSubmissionModel : PageModel
    {
        private readonly PortFreightContext _context;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly ILogger<EditSubmissionModel> _logger;

        public Msd1Data msd1;

        [BindProperty]
        public uint Imo { get; set; }
        [BindProperty]
        public string ShipName { get; set; }
        [BindProperty]
        public uint Year { get; set; }
        [BindProperty]
        public ushort Quarter { get; set; }
        [BindProperty]
        public uint NumVoyages { get; set; }
        [BindProperty]
        public string ReportingPort { get; set; }
        [BindProperty]
        public string AssociatedPort { get; set; }
        [BindProperty]
        public bool IsInbound { get; set; }

        public EditSubmissionModel(PortFreightContext context,
            UserManager<PortFreightUser> userManager,
            ILogger<EditSubmissionModel> logger,
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
                msd1 = TempData.GetKeep<Msd1Data>("Msd1");
                IsInbound = msd1.IsInbound;
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

            mapInputsToMSDI(user);

            if (doAllPortsAndShipsExist())
            {
                try
                {
                    _context.Msd1Data.Update(msd1);
                    _context.SaveChanges();

                    TempData.Put("EditSuccess", "Submission has been successfully amended");

                    return RedirectToPage("/Submissions/FindSubmission", new { area = "Admin" });
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);

                    ModelState.AddModelError("CustomError", "There is a problem save the changes");

                    return Page();
                }
            }

            return Page();

        }

        private bool doesShipExist()
        {
            try
            {
                return _context.WorldFleet.Any(x => x.Imo == msd1.Imo);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool doesShipNameIMOMatch()
        {
            try
            {
                return _context.WorldFleet.Any(x => x.Imo == msd1.Imo && x.ShipName == msd1.ShipName);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool doesPortExist(string locode)
        {
            try
            {
                return _context.GlobalPort.Any(x => x.Locode == locode);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void mapInputsToMSDI(PortFreightUser user)
        {
            msd1 = TempData.GetKeep<Msd1Data>("Msd1");

            msd1.Imo = Imo;
            msd1.ShipName = ShipName;
            msd1.Year = Year;
            msd1.Quarter = Quarter;
            msd1.NumVoyages = NumVoyages;
            msd1.ReportingPort = ReportingPort;
            msd1.AssociatedPort = AssociatedPort;
            msd1.LastUpdatedBy = user.Email;
            msd1.ModifiedDate = DateTime.Now;
            msd1.IsInbound = IsInbound;
        }

        private bool doAllPortsAndShipsExist()
        {
            bool shipExists = doesShipExist();
            bool shipNameIMOMatch = doesShipNameIMOMatch();
            bool reportingPortExists = doesPortExist(msd1.ReportingPort);
            bool associatedPortExists = doesPortExist(msd1.AssociatedPort);

            if (!shipExists)
            {
                ModelState.AddModelError("IMO", "The IMO entered is not in the world fleet list");
            }

            if (!shipNameIMOMatch)
            {
                ModelState.AddModelError("ShipName", "The IMO and Ship Name entered do not match in the world fleet list");
            }

            if (!reportingPortExists)
            {
                ModelState.AddModelError("ReportingPort", "The Reporting port entered is not in the global port list");
            }

            if (!associatedPortExists)
            {
                ModelState.AddModelError("AssociatedPort", "The Associated port entered is not in the global port list");
            }


            if (shipExists && shipNameIMOMatch && reportingPortExists && associatedPortExists)
            {
                return true;
            }

            return false;
        }
    }
}