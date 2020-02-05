using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Areas.Admin.Pages.Submissions
{
    public class FindSubmissionModel : PageModel
    {
        #region publicProperties 
        public string SuccessMessage { get; private set; } = string.Empty;
        public List<string> StatPortsList { get; set; }
        [BindProperty]
        public SubmissionSearchInputModel Input { get; set; }
        public List<Msd1SummaryViewModel> Msd1SearchResultList { get; private set; } = new List<Msd1SummaryViewModel>();
        #endregion

        #region privateVariables 
        private readonly PortFreightContext _context;
        private readonly ILogger<FindSubmissionModel> _logger;
        private readonly string FindSubKey = "FindSubmissionInput";       
        private readonly ICsvExtract _csvExtractor;
        private readonly ISubmissionSearch _msd1SubmissionSearch;
        #endregion

        public FindSubmissionModel(PortFreightContext context,
                                    ILogger<FindSubmissionModel> logger,
                                    ICsvExtract csvExtractor,
                                    ISubmissionSearch msd1SubmissionSearch)
        {
            _context = context;
            _logger = logger;
            _csvExtractor = csvExtractor;
            _msd1SubmissionSearch = msd1SubmissionSearch;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                if (TempData.NotNullOrEmpty() && TempData.Peek(FindSubKey) != null)
                {
                    Input = TempData.Get<SubmissionSearchInputModel>(FindSubKey);
                    TempData.Keep(FindSubKey);
                }
                else
                {
                    Input = new SubmissionSearchInputModel();
                }
                await PopulateDropDowns();
                SuccessMessage = TempData.Get<string>("EditSuccess") ?? TempData.Get<string>("DeleteSuccess") ?? string.Empty;
                Msd1SearchResultList = _msd1SubmissionSearch.GenerateMsd1ViewModelFilteredOnSearchCriteria(Input, false);
            }
           
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", "There has been an error connecting to the database");
                _logger.LogError(e.Message);
            }
            return Page();
        }

        public IActionResult OnGetViewSummary(string id)
        {
            if (TempData.NotNullOrEmpty() && TempData.Peek(FindSubKey) != null)
            {
                Input = TempData.Get<SubmissionSearchInputModel>(FindSubKey);
                TempData.Keep(FindSubKey);
            }
         
            TempData["Msd1Id"] = id;
            return RedirectToPage("/Summaries/ViewSummary", new { area = "Admin" });
        }

        public IActionResult OnGetEditSubmission(string id)
        {
            if (TempData.NotNullOrEmpty() && TempData.Peek(FindSubKey) != null)
            {
                Input = TempData.Get<SubmissionSearchInputModel>(FindSubKey);
                TempData.Keep(FindSubKey);
            }
            TempData.Put("Msd1", _context.Msd1Data.FirstOrDefault(x => x.Msd1Id == id));
            return RedirectToPage("/Submissions/EditSubmission", new { area = "Admin" });
        }

        public IActionResult OnGetDeleteSubmission(string id)
        {
            if (TempData.NotNullOrEmpty() && TempData.Peek(FindSubKey) != null)
            {
                Input = TempData.Get<SubmissionSearchInputModel>(FindSubKey);
                TempData.Keep(FindSubKey);
            }
            TempData.Put("Msd1", _context.Msd1Data.FirstOrDefault(x => x.Msd1Id == id));
            return RedirectToPage("/Submissions/DeleteSubmission", new { area = "Admin" });
        }

        public async Task<IActionResult> OnPostSearch()
        {
            try
            {
                if (Input.DateEntered != null){
                    if (!ValidateDate()){
                        ModelState.AddModelError("CustomError", "Please check the format of your date");
                        Input = new SubmissionSearchInputModel();
                    }
                }

                Msd1SearchResultList = _msd1SubmissionSearch.GenerateMsd1ViewModelFilteredOnSearchCriteria(Input, false);
                await PopulateDropDowns();
                if (Msd1SearchResultList.Count == 0)
                {
                    ModelState.AddModelError("CustomError", "No records have been found matching the above criteria");
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", "There has been an error");
                _logger.LogError(e.Message);
            }

            TempData.Put(FindSubKey, Input);
            return Page();
        }

        public IActionResult OnGetExport(SubmissionSearchInputModel searchInputs)
        {
            var msd1SummarySearchResultList = _msd1SubmissionSearch.GenerateMsd1ViewModelFilteredOnSearchCriteria( searchInputs, true);                    
            var byteArrayCsvData = _csvExtractor.GenerateMsd1CsvExtract(msd1SummarySearchResultList);
            var csvDownloadFileName = "MSD1SearchResults" + DateTime.Now.ToShortDateString() + ".csv";
            return File(byteArrayCsvData, "text/csv", csvDownloadFileName);
        }
           

        #region privateMethods
        private async Task PopulateDropDowns()
        {
            PopulateCargoType();
            await CreateStatPortList();
        }

        private void PopulateCargoType()
        {
            Input.CargoType = _context.CargoGroup.AsNoTracking().Select(x => new SelectListItem
            {
                Value = x.GroupCode.ToString(),
                Text = x.Description
            }).ToList();
        }


        private bool ValidateDate()
        {
            char[] splitDateChars = {'/' , '-'};
            var date = Input.DateEntered.ToString();
            var month = Int32.Parse(date.Split(splitDateChars)[1]);
            if (month <= 12)
            {
                return true;
            }
            return false;       
        }
        
        private async Task<List<string>> CreateStatPortList()
        {
            StatPortsList = await _context.GlobalPort
                .Where(x => x.ForMsd1ReportingPort)
                .OrderByDescending(x => x.StatisticalPort)
                .Select(
                    s =>
                        s.StatisticalPort
                ).Distinct().ToListAsync();

            StatPortsList.Reverse();
            return StatPortsList;
        }
        #endregion

    }
}
