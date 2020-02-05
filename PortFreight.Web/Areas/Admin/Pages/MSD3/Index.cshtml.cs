using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Areas.Admin.Pages.MSD3
{
    public class IndexModel : PageModel
    {
        private readonly PortFreight.Data.PortFreightContext _context;
        private readonly ILogger<IndexModel> _logger;
        private Dictionary<string, string> searchFilterDictionary = new Dictionary<string, string>();
        private ICsvExtract _csvExtractor;
        private string FindSubKey { get; set; } = "FindSubmissionInput";
        private readonly ISubmissionSearch _msd3SubmissionSearch;

        [BindProperty]
        public Msd3SearchInputModel SearchDataInputModel { get; set; }
        public List<Data.Entities.GlobalPort> ListOfPorts;
        public List<string> StatPortsList { get; set; }
    
        public List<Msd3SummaryViewModel> Msd3SummarySearchResultList = new List<Msd3SummaryViewModel>();

        public string SuccessMessage { get; private set; } = string.Empty;
        
        public IndexModel(PortFreight.Data.PortFreightContext context, ILogger<IndexModel> logger, ISubmissionSearch msd3SubmissionSearch, ICsvExtract csvExtractor)
        {
            _context = context;
            _logger = logger;
             _csvExtractor = csvExtractor;
            _msd3SubmissionSearch = msd3SubmissionSearch;
        }
        public async Task<IActionResult> OnGetAsync(string message)
        {
            try
            {
                SearchDataInputModel = new Msd3SearchInputModel();
                if (TempData.NotNullOrEmpty() && TempData.Peek(FindSubKey) != null)
                {
                    SearchDataInputModel = TempData.Get<Msd3SearchInputModel>(FindSubKey);
                    TempData.Keep(FindSubKey);
                }
                await PopulateDropDowns();
                SuccessMessage = TempData.Get<string>("EditSuccess") ?? TempData.Get<string>("DeleteSuccess") ?? string.Empty;             
                Msd3SummarySearchResultList = _msd3SubmissionSearch.GenerateMsd3ViewModelFilteredOnSearchCriteria(SearchDataInputModel, false);

            }
            catch (NullReferenceException)
            {
                SearchDataInputModel = new Msd3SearchInputModel();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", "There has been an error connecting to the database");
                _logger.LogError(e.Message);
            }
            return Page();
        }

         public async Task<IActionResult> OnPostSearch(){
            try
            {
                if (SearchDataInputModel.DateEntered != null){
                    if (!ValidateDate()){
                        ModelState.AddModelError("CustomError", "Please check the format of your date");
                        SearchDataInputModel = new Msd3SearchInputModel();
                    }
                }

                Msd3SummarySearchResultList =  _msd3SubmissionSearch.GenerateMsd3ViewModelFilteredOnSearchCriteria(SearchDataInputModel, false);
                await PopulateDropDowns();
                if (Msd3SummarySearchResultList.Count == 0)
                {
                    ModelState.AddModelError("CustomError", "No records have been found matching the above criteria");
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", e.Message);
                _logger.LogError(e.Message);
            }

            TempData.Put(FindSubKey, SearchDataInputModel);           
            return Page();
        }

        public async Task<IActionResult> OnGetExport(Msd3SearchInputModel searchInput)
        {
            var msd3SummarySearchResultList = _msd3SubmissionSearch.GenerateMsd3ViewModelFilteredOnSearchCriteria(searchInput,true);
            await PopulateDropDowns();
            if (msd3SummarySearchResultList.Count == 0)
            {
                ModelState.AddModelError("CustomError", "No records have been found matching the above criteria");
                return Page();
            }
            var byteArrayCsvData =_csvExtractor.GenerateMsd3CsvExtract(msd3SummarySearchResultList);
            var csvDownloadFileName = "MSD3SearchResults" + DateTime.Now.ToShortDateString() + ".csv";
            return File(byteArrayCsvData, "text/csv", csvDownloadFileName);
        }

        public IActionResult OnGetViewSummary(string id)
        {
            TempData.Put(FindSubKey, SearchDataInputModel);
            TempData["Msd3Id"] = id;
            return RedirectToPage("/Summaries/ViewSummary", new { area = "Admin" });
        }

        public IActionResult OnGetEditSubmission(string id)
        {

            TempData.Put(FindSubKey, SearchDataInputModel);
            TempData.Put("Msd3", _context.Msd3.FirstOrDefault(x => x.Id.ToString() == id));
            return RedirectToPage("/Submissions/EditSubmission", new { area = "Admin" });
        }

        public IActionResult OnPostDeleteSubmission(string id)
        {
            TempData.Put(FindSubKey, SearchDataInputModel);
            TempData.Put("Msd3", _context.Msd3.FirstOrDefault(x => x.Id.ToString() == id));
            return RedirectToPage("/Submissions/DeleteSubmission", new { area = "Admin" });
        }

        private async Task PopulateDropDowns()
        {
            await CreateStatPortList();
        }

        private bool ValidateDate()
        {
            char[] splitDateChars = {'/' , '-'};
            var date = SearchDataInputModel.DateEntered.ToString();
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
                .Where(x => x.ForMsd2)
                .OrderByDescending(x => x.StatisticalPort)
                .Select(
                    s =>
                        s.StatisticalPort
                ).Distinct().ToListAsync();

            StatPortsList.Reverse();
            return StatPortsList;
        }
        
    }
}
