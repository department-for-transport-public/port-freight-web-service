using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Services.MSD2;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Pages.QuarterlyReturn
{
    public class CargoReturnModel : BaseMsd1PageModel
    {
        private readonly PortFreightContext _context;
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public DateTime currentDateTime = DateTime.Now;

        public PortFreightUser LoggedInUser { get; set; }
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly IHelperService _helperService;
        private readonly IMsd2DataService _msd2DataService;
        public MSD23 MSD23 { get; set; }

        [BindProperty]
        public bool IsRecordExist { get; private set; }

        [BindProperty]
        public bool IsInwardBoxVisible { get; private set; }
        [BindProperty]
        public decimal? PreviousYearGWTInward { get; private set; }

        [BindProperty]
        public bool IsOutwardBoxVisible { get; private set; }
        [BindProperty]
        public decimal? PreviousYearGWTOutward { get; private set; }

        [BindProperty]
        public bool IsInwardUnitBoxVisible { get; private set; }
        [BindProperty]
        public uint? PreviousYearUintInward { get; private set; }

        [BindProperty]
        public bool IsOutwardUnitBoxVisible { get; private set; }
        [BindProperty]
        public uint? PreviousYearUintOutward { get; private set; }

        public CargoReturnModel(PortFreightContext context, UserManager<PortFreightUser> userManager, IHelperService helperService, IMsd2DataService msd2DataService)
        {
            _context = context;
            _userManager = userManager;
            _helperService = helperService;
            _msd2DataService = msd2DataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            LoggedInUser = await _userManager.GetUserAsync(HttpContext.User);
            InitialisePage(LoggedInUser);
            IsRecordExist = false;
            IsInwardBoxVisible = false;
            IsOutwardBoxVisible = false;
            IsInwardUnitBoxVisible = false;
            IsOutwardUnitBoxVisible = false;

            var existingParent = FindSubmission();
            if (existingParent != null)
            {
                IsRecordExist = true;
                AssignMSD2ToInput(existingParent);

                PreviousYearGWTInward = _context.Msd2.
                    Where(p => p.ReportingPort == existingParent.ReportingPort && p.Year == existingParent.Year - 1 && p.Quarter == existingParent.Quarter && p.SenderId == LoggedInUser.SenderId)
                    .Select(g => g.GrossWeightInward).SingleOrDefault();

                PreviousYearGWTOutward = _context.Msd2.
                    Where(p => p.ReportingPort == existingParent.ReportingPort && p.Year == existingParent.Year - 1 && p.Quarter == existingParent.Quarter && p.SenderId == LoggedInUser.SenderId)
                    .Select(g => g.GrossWeightOutward).SingleOrDefault();

                PreviousYearUintInward = _context.Msd2.
                    Where(p => p.ReportingPort == existingParent.ReportingPort && p.Year == existingParent.Year - 1 && p.Quarter == existingParent.Quarter && p.SenderId == LoggedInUser.SenderId)
                    .Select(g => g.TotalUnitsInward).SingleOrDefault();

                PreviousYearUintOutward = _context.Msd2.
                    Where(p => p.ReportingPort == existingParent.ReportingPort && p.Year == existingParent.Year - 1 && p.Quarter == existingParent.Quarter && p.SenderId == LoggedInUser.SenderId)
                    .Select(g => g.TotalUnitsOutward).SingleOrDefault();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoggedInUser = await _userManager.GetUserAsync(HttpContext.User);
            MSD23 = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD23Key] != null ? new MSD23(TempData.GetKeep<MSD23>(MSD23Key)) : new MSD23();

            if (Input.PassengerVehiclesInwards > Input.UnitsInwards)
            {
                ModelState.AddModelError("Input.PassengerVehiclesInwards", $"Passenger Vehicle Inwards cannot be greater than total number of units inwards");
            }
            if (Input.PassengerVehiclesOutwards > Input.UnitsOutwards)
            {
                ModelState.AddModelError("Input.PassengerVehiclesOutwards", $"Passenger Vehicle Outwards cannot be greater than total number of units outwards");
            }
            if (Input.GrossWeightInwards != null)
            {
                var (previousYearGWTInward, thresoldInward, boxVisibleInward) = _msd2DataService.ValidateGrossWeightInwards((decimal)Input.GrossWeightInwards, MSD23.Year, MSD23.Quarter, MSD23.Port, LoggedInUser.SenderId);
                PreviousYearGWTInward = previousYearGWTInward;
                if (boxVisibleInward)
                {
                    IsInwardBoxVisible = boxVisibleInward;
                    if (Input.GrossWeightInwardsDescription == null)
                        ModelState.AddModelError("Input_GrossWeightInwardsDescription", $"Check gross weight (inwards) and enter description if correct");
                }
                else
                {
                    ModelState.Remove("Input_InwardsUnitDescription");
                    Input.GrossWeightInwardsDescription = null;
                }
            }
            if (Input.GrossWeightOutwards != null)
            {
                var (previousYearGWTOutward, thresoldOutward, boxVisibleOutward) = _msd2DataService.ValidateGrossWeightOutwards((decimal)Input.GrossWeightOutwards, MSD23.Year, MSD23.Quarter, MSD23.Port, LoggedInUser.SenderId);
                PreviousYearGWTOutward = previousYearGWTOutward;
                if (boxVisibleOutward)
                {
                    IsOutwardBoxVisible = boxVisibleOutward;
                    if (Input.GrossWeightOutwardsDescription == null)
                        ModelState.AddModelError("Input_GrossWeightOutwardsDescription", $"Check gross weight (outwards) and enter description if correct");
                }
                else
                {
                    Input.GrossWeightOutwardsDescription = null;
                }
            }

            if (Input.UnitsInwards != null)
            {
                var (previousYearUnitsInward, thresoldInward, boxVisibleUintInward) = _msd2DataService.ValidateUnitsInwards(Input.UnitsInwards, MSD23.Year, MSD23.Quarter, MSD23.Port, LoggedInUser.SenderId);
                PreviousYearUintInward = previousYearUnitsInward;
                if (boxVisibleUintInward)
                {
                    IsInwardUnitBoxVisible = boxVisibleUintInward;
                    if (Input.InwardsUnitDescription == null)
                        ModelState.AddModelError("Input_InwardsUnitDescription", $"Check units (inwards) and enter description if correct");
                }
                else
                {
                    Input.InwardsUnitDescription = null;
                }
            }

            if (Input.UnitsOutwards != null)
            {
                var (previousYearUnitsOutward, thresoldOutward, boxVisibleUintOutward) = _msd2DataService.ValidateUnitsOutwards(Input.UnitsOutwards, MSD23.Year, MSD23.Quarter, MSD23.Port, LoggedInUser.SenderId);
                PreviousYearUintOutward = previousYearUnitsOutward;
                if (boxVisibleUintOutward)
                {
                    IsOutwardUnitBoxVisible = boxVisibleUintOutward;
                    if (Input.OutwardsUnitDescription == null)
                        ModelState.AddModelError("Input_OutwardsUnitDescription", $"Check units (outwards) and enter description if correct");
                }
                else
                {
                    Input.OutwardsUnitDescription = null;
                }
            }

            if (!ModelState.IsValid)
            {
                InitialisePage(LoggedInUser);
                return Page();
            }
            var MSD2Data = new Msd2
            {
                SenderId = LoggedInUser.SenderId,
                ReportingPort = MSD23.Port,
                Year = MSD23.Year,
                Quarter = MSD23.Quarter,
                GrossWeightInward = (decimal)Input.GrossWeightInwards,
                InwardGrossWeightDescription = Input.GrossWeightInwardsDescription,
                TotalUnitsInward = (uint)Input.UnitsInwards,
                InwardUnitDescription = Input.InwardsUnitDescription,
                PassengerVehiclesInward = Input.UnitsInwards == 0 ? 0 : Input.PassengerVehiclesInwards ?? 0,
                GrossWeightOutward = (decimal)Input.GrossWeightOutwards,
                OutwardGrossWeightDescription = Input.GrossWeightOutwardsDescription,
                TotalUnitsOutward = (uint)Input.UnitsOutwards,
                OutwardUnitDescription = Input.OutwardsUnitDescription,
                PassengerVehiclesOutward = Input.UnitsOutwards == 0 ? 0 : Input.PassengerVehiclesOutwards ?? 0,
                DataSourceId = (uint)DataSource.WEB,
                CreatedDate = DateTime.SpecifyKind(currentDateTime, DateTimeKind.Utc),
                ModifiedDate = DateTime.SpecifyKind(currentDateTime, DateTimeKind.Utc),
                CreatedBy = LoggedInUser.Email.ToString(),
                LastUpdatedBy = LoggedInUser.Email.ToString()
            };
            var existingParent = FindSubmission();

            if (existingParent != null)
            {
                MSD2Data.Id = existingParent.Id;
                MSD2Data.ReportingPort = existingParent.ReportingPort;
                MSD2Data.Year = existingParent.Year;
                MSD2Data.Quarter = existingParent.Quarter;
                MSD2Data.DataSourceId = existingParent.DataSourceId;
                MSD2Data.CreatedDate = existingParent.CreatedDate;
                MSD2Data.CreatedBy = existingParent.CreatedBy;
                _context.Entry(existingParent).CurrentValues.SetValues(MSD2Data);
            }
            else
            {
                await _context.AddAsync(MSD2Data);
            }
            await _context.SaveChangesAsync();

            return RedirectToPage("./Success");
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Enter the gross weight inwards (tonnes)")]
            [RegularExpression(@"^\s*(?=.*[0-9])\d*(?:\.\d{1,3})?\s*$", ErrorMessage = "Limit to 3 decimal places and enter a value greater than or equal to '0'")]
            [Display(Name = "Gross weight of goods inwards (tonnes)")]
            public double? GrossWeightInwards { get; set; }
            [Display(Name = "Description")]
            public string GrossWeightInwardsDescription { get; set; }
            [Required(ErrorMessage = "Enter the number of units inwards. Type '0' if all inwards goods were bulk")]
            [Display(Name = "Total number of units inwards")]
            public uint? UnitsInwards { get; set; }
            [Display(Name = "Description")]
            public string InwardsUnitDescription { get; set; }
            [Display(Name = "Of these, how many were passenger vehicles not for trade?")]
            public uint? PassengerVehiclesInwards { get; set; }

            [Required(ErrorMessage = "Enter the gross weight outwards (tonnes)")]
            [RegularExpression(@"^\s*(?=.*[0-9])\d*(?:\.\d{1,3})?\s*$", ErrorMessage = "Limit to 3 decimal places and enter a value greater than or equal to '0'")]
            [Display(Name = "Gross weight of goods outwards (tonnes)")]
            public double? GrossWeightOutwards { get; set; }
            [Display(Name = "Description")]
            public string GrossWeightOutwardsDescription { get; set; }
            [Required(ErrorMessage = "Enter the number of units outwards. Type '0' if all outward goods were bulk")]
            [Display(Name = "Total number of units outwards")]
            public uint? UnitsOutwards { get; set; }
            [Display(Name = "Description")]
            public string OutwardsUnitDescription { get; set; }
            [Display(Name = "Of these, how many were passenger vehicles not for trade?")]
            public uint? PassengerVehiclesOutwards { get; set; }
        }

        private void InitialisePage(PortFreightUser user)
        {
            MSD23 = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD23Key] != null ? new MSD23(TempData.GetKeep<MSD23>(MSD23Key)) : new MSD23();
            var locodes = (from p in _context.SenderIdPort
                           join t in _context.SenderType on p.SenderId equals t.SenderId
                           where (t.IsPort == true) && p.SenderId == user.SenderId
                           select p.Locode).ToList();
        }

        private Msd2 FindSubmission()
        {
            return _context.Msd2
                .Where(p => p.ReportingPort == MSD23.Port
                && p.Year == MSD23.Year
                && p.Quarter == MSD23.Quarter
                && p.SenderId == LoggedInUser.SenderId).SingleOrDefault();
        }

        private void AssignMSD2ToInput(Msd2 msd2)
        {
            MSD23 = new MSD23(TempData.GetKeep<MSD23>(MSD23Key));
            MSD23.Year = msd2.Year;
            MSD23.Quarter = msd2.Quarter;
            MSD23.Port = msd2.ReportingPort;
            MSD23.PortName = _helperService.GetPortNameByCode(msd2.ReportingPort);
            Input.GrossWeightInwards = (double)msd2.GrossWeightInward;
            Input.UnitsInwards = msd2.TotalUnitsInward;
            Input.InwardsUnitDescription = msd2.InwardUnitDescription;
            Input.PassengerVehiclesInwards = msd2.PassengerVehiclesInward;
            Input.GrossWeightOutwards = (double)msd2.GrossWeightOutward;
            Input.UnitsOutwards = msd2.TotalUnitsOutward;
            Input.OutwardsUnitDescription = msd2.OutwardUnitDescription;
            Input.PassengerVehiclesOutwards = msd2.PassengerVehiclesOutward;
            Input.GrossWeightInwardsDescription = msd2.InwardGrossWeightDescription;
            Input.GrossWeightOutwardsDescription = msd2.OutwardGrossWeightDescription;
            IsInwardBoxVisible = msd2.InwardGrossWeightDescription == null ? false : true;
            IsInwardUnitBoxVisible = msd2.InwardUnitDescription == null ? false : true;
            IsOutwardBoxVisible = msd2.OutwardGrossWeightDescription == null ? false : true;
            IsOutwardUnitBoxVisible = msd2.OutwardUnitDescription == null ? false : true;
        }
    }
}