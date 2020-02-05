using System;
using System.Web;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Pages.Msd1
{
    public class VoyageDetailsModel : BaseMsd1PageModel
    {
        private readonly PortFreightContext _context;
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public MSD1 MSD1 { get; set; }
        public List<SelectListItem> Years { get; set; }
        public List<SelectListItem> Quarters { get; set; }
        public List<string> ReportingPorts { get; set; }
        public List<string> AssociatedPorts { get; set; }
        
        public string ReportingPort { get; set; }
        private readonly IHelperService _helperService;
        [BindProperty]
        public bool FromSummary { get; set; }
        
        

        public VoyageDetailsModel(PortFreightContext context,
            IHelperService helperService)
        {
            _context = context;
            _helperService = helperService;
        }

        public IActionResult OnGet()
        {
            InitialisePage();
            AssignMSD1ToInput();
            TempData.Put("AP", "");
            TempData.Put("RP","");

            return Page();
        }

        public IActionResult OnPost(string reportingport, string associatedport)
        {  

            ModelState.Clear();
            Input.ReportingPort = reportingport;
            Input.AssociatedPort = associatedport;

            TempData.Put("AP", associatedport);
            TempData.Put("RP", reportingport);

            TryValidateModel(Input);

            if (!ModelState.IsValid)
            {
                InitialisePage();
                return Page();
            }

            AssignInputToMSD1();

            TempData.Put(MSD1Key, MSD1);

            return RedirectToPage("./CargoDetails", new { FromSummary = FromSummary.ToString(), IsEdited = FromSummary.ToString() });
        }

        private void InitialisePage()
        {
            Years = new List<SelectListItem>()
            {
                new SelectListItem
                {
                Text = DateTime.Now.Year.ToString(), Value = DateTime.Now.Year.ToString()
                },
                new SelectListItem
                {
                Text = DateTime.Now.AddYears(-1).Year.ToString(), Value = DateTime.Now.AddYears(-1).Year.ToString()
                }
            };

            Quarters = new List<SelectListItem>()
            {
                new SelectListItem
                {
                Text = "Select a Quarter", Value = "0", Disabled = true
                },
                new SelectListItem
                {
                Text = "Quarter 1", Value = "1"
                },
                new SelectListItem
                {
                Text = "Quarter 2", Value = "2"
                },
                new SelectListItem
                {
                Text = "Quarter 3", Value = "3"
                },
                new SelectListItem
                {
                Text = "Quarter 4", Value = "4"
                },

            };

            ReportingPorts = _helperService.GetReportingPorts();
            ReportingPort = Input.ReportingPort;
           

            AssociatedPorts = _helperService.GetPortsOfLoadUnload();
            if (TempData[MSD1Key] == null && TempData[MSD1Success] != null)
            {
                MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Success));
                TempData.Put(MSD1Key, MSD1);
                TempData.Remove(MSD1Success);
            }

            MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));

            FromSummary = Helpers.ReturnBoolFromQueryString(HttpContext, "FromSummary");
        }

        public DateTime? ConcatenateDate()
        {
            int? voyageDateDay = Input.VoyageDateDay;
            int? voyageDateMonth = Input.VoyageDateMonth;
            int? voyageDateYear = Input.VoyageDateYear;
            DateTime voyageDateParsed;
            if (voyageDateDay != null && voyageDateMonth != null && voyageDateYear != null)
            {
                string voyageDate = voyageDateYear.ToString() + "-" + voyageDateMonth.ToString() + "-" + voyageDateDay.ToString();

                if (DateTime.TryParse(voyageDate, out voyageDateParsed))
                {
                    return voyageDateParsed;
                }
            }

            return null;

        }

        private void AssignInputToMSD1()
        {
            MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));

            if (!FromSummary && (MSD1.AssociatedPort != Input.AssociatedPort || MSD1.ReportingPort != Input.ReportingPort))
            {
                MSD1.CargoSummary.Clear();
            }

            MSD1.Year = uint.Parse(Input.Year);
            MSD1.Quarter = ushort.Parse(Input.Quarter);
            MSD1.ReportingPort = Input.ReportingPort;
            MSD1.IsInbound = Input.IsInbound;
            MSD1.AssociatedPort = Input.AssociatedPort;
            MSD1.NumVoyages = uint.Parse(Input.NumVoyages);
            MSD1.RecordRef = Input.RecordRef;
            MSD1.VoyageDate = ConcatenateDate();
        }

        private void AssignMSD1ToInput()
        {
            if (CustomExtensions.NotNullOrEmpty(TempData))
            {
                MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));

                Input.Year = MSD1.Year.ToString();
                Input.Quarter = MSD1.Quarter.ToString();
                Input.ReportingPort = MSD1.ReportingPort;
                Input.IsInbound = MSD1.IsInbound;
                Input.AssociatedPort = MSD1.AssociatedPort;
                Input.NumVoyages = MSD1.NumVoyages.ToString();
                Input.RecordRef = MSD1.RecordRef;

            }
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Enter a valid reporting port")]
            public string ReportingPort { get; set; }

            [Required(ErrorMessage = "Select a direction of travel")]
            public bool? IsInbound { get; set; } = null;

            [AssociatedPort()]
            [Required(ErrorMessage = "Enter a valid port")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "Enter a valid port")]
            public string AssociatedPort { get; set; }

            [NumVoyages()]
            public string NumVoyages { get; set; } = "1";

            [Required(ErrorMessage = "Select a year")]
            public string Year { get; set; }

            [Quarter()]
            [Required(ErrorMessage = "Select a quarter")]
            public string Quarter { get; set; }

            [StringLength(20, ErrorMessage = "Optional reference cannot be longer than 20 characters")]
            public string RecordRef { get; set; }

            [DateDay()]
            public int? VoyageDateDay { get; set; }

            [DateMonth()]
            public int? VoyageDateMonth { get; set; }

            [DateCheckQuarter()]
            [DateYear()]
            [ValidateEitherNoneOrAllEntriesAreFilled()]
            public int? VoyageDateYear { get; set; }

        }

        public class NumVoyagesAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                InputModel Input = (InputModel)validationContext.ObjectInstance;

                if (int.TryParse(Input.NumVoyages, out int numOfVoyages) && numOfVoyages > 0 && numOfVoyages <= 1000)
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult("Enter number of voyages between 1 - 1000");
            }

        }
        public class AssociatedPortAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                InputModel Input = (InputModel)validationContext.ObjectInstance;

                if (Input.ReportingPort != Input.AssociatedPort)
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult("The two ports must be different");
            }
        }

        public class QuarterAttribute : ValidationAttribute
        {
            public int currentQuarter;
            public string currentYear;

            public QuarterAttribute()
            {
                currentQuarter = Convert.ToInt16(decimal.Round((DateTime.Now.Month / 3m) + 0.5m, 0, MidpointRounding.AwayFromZero));
                currentYear = Convert.ToString(DateTime.Now.Year);
            }

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                InputModel Input = (InputModel)validationContext.ObjectInstance;
                int inputQuarter = int.Parse(Input.Quarter);

                if (inputQuarter == 0 )
                {
                    return new ValidationResult("Enter a Quarter");
                }
                 

                if (inputQuarter > currentQuarter && Input.Year == currentYear)
                {
                    return new ValidationResult("Voyage must be in the past");
                }



                return ValidationResult.Success;
            }
        }

        public class ValidateEitherNoneOrAllEntriesAreFilled : ValidationAttribute
        {

            public int? voyageDateDay;
            public int? voyageDateMonth;
            public int? voyageDateYear;

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                InputModel Input = (InputModel)validationContext.ObjectInstance;

                int? voyageDateDay = Input.VoyageDateDay;
                int? voyageDateMonth = Input.VoyageDateMonth;
                int? voyageDateYear = Input.VoyageDateYear;

                if (voyageDateDay != null)
                {
                    if (voyageDateMonth == null || voyageDateYear == null)
                    {
                        return new ValidationResult("You must either leave all date fields blank or fill all date fields in");
                    }
                }
                else if (voyageDateMonth != null)
                {
                    if (voyageDateDay == null || voyageDateYear == null)
                    {
                        return new ValidationResult("You must either leave all date fields blank or fill all date fields in");
                    }
                }
                else if (voyageDateYear != null)
                {
                    if (voyageDateMonth == null || voyageDateDay == null)
                    {
                        return new ValidationResult("You must either leave all date fields blank or fill all date fields in");
                    }
                }

                return ValidationResult.Success;
            }
        }

        public class DateDayAttribute : ValidationAttribute
        {
            public int? voyageDateDay;

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {

                InputModel Input = (InputModel)validationContext.ObjectInstance;

                int? voyageDateDay = Input.VoyageDateDay;

                if (voyageDateDay > 31 || voyageDateDay <= 0)
                {
                    return new ValidationResult("The day must be between 1 and 31");
                }
                return ValidationResult.Success;
            }
        }

        public class DateMonthAttribute : ValidationAttribute
        {
            public int? voyageDateMonth;
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                InputModel Input = (InputModel)validationContext.ObjectInstance;
                int? voyageDateMonth = Input.VoyageDateMonth;
                if (voyageDateMonth > 12 || voyageDateMonth <= 0)
                {
                    return new ValidationResult("The month must be between 1 and 12");
                }
                return ValidationResult.Success;
            }
        }

        public class DateYearAttribute : ValidationAttribute
        {
            public int? voyageDateYear;
            public int year;
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                InputModel Input = (InputModel)validationContext.ObjectInstance;
                voyageDateYear = Input.VoyageDateYear;
                int year = Convert.ToInt16(Input.Year);
                if (voyageDateYear != null && voyageDateYear != year)
                {
                    return new ValidationResult("The year does not match the year you previously selected");
                }
                return ValidationResult.Success;
            }
        }

        public class DateCheckQuarterAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                InputModel Input = (InputModel)validationContext.ObjectInstance;
                if (Input.Quarter == "1")
                {
                    if (Input.VoyageDateMonth > 3)
                    {
                        return new ValidationResult("The date entered is not within Quarter 1");
                    }
                }
                else if (Input.Quarter == "2")
                {
                    if (Input.VoyageDateMonth < 4 || Input.VoyageDateMonth > 6)
                    {
                        return new ValidationResult("The date entered is not within Quarter 2");
                    }
                }
                else if (Input.Quarter == "3")
                {
                    if (Input.VoyageDateMonth < 6 || Input.VoyageDateMonth > 9)
                    {
                        return new ValidationResult("The date entered is not within Quarter 3");
                    }
                }
                else if (Input.Quarter == "4")
                {
                    if (Input.VoyageDateMonth < 9 || Input.VoyageDateMonth > 12)
                    {
                        return new ValidationResult("The date entered is not within Quarter 4");
                    }
                }
                return ValidationResult.Success;
            }
        }

    }
}
