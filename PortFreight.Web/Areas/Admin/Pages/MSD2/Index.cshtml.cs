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

namespace PortFreight.Web.Areas.Admin.Pages.MSD2
{
    public class IndexModel : PageModel
    {
        #region privateVariables
        private readonly PortFreightContext _context;
        private readonly ILogger<IndexModel> _logger;
        private readonly ICsvExtract _csvExtractor;
        private readonly string FindSubKey = "FindSubmissionInput";
        private readonly ISubmissionSearch _msd2SubmissionSearch;
        #endregion

        #region publicVariables
        [BindProperty]
        public Msd2SearchInputModel SearchDataInputModel { get; set; }
               
        public List<string> StatPortsList { get; set; }
        public List<Msd2SummaryViewModel> Msd2SummarySearchResultList { get; private set; } = new List<Msd2SummaryViewModel>();
        public string SuccessMessage { get; private set; }
        #endregion

        public IndexModel(PortFreightContext context, 
                            ILogger<IndexModel> logger, 
                            ISubmissionSearch msd2SubmissionSearch,  
                            ICsvExtract csvExtractor)
        {
            _context = context;
            _logger = logger;
             _csvExtractor = csvExtractor;
            _msd2SubmissionSearch = msd2SubmissionSearch;
        }

        public async Task <IActionResult> OnGetAsync()
        {
             try
            {
                if (TempData.NotNullOrEmpty() && TempData.Peek(FindSubKey) != null)
                {
                    SearchDataInputModel = TempData.Get<Msd2SearchInputModel>(FindSubKey);
                    TempData.Keep(FindSubKey);
                }
                else {
                    SearchDataInputModel = new Msd2SearchInputModel();
                }
                await PopulateDropDowns();
                SuccessMessage = TempData.Get<string>("EditSuccess") ?? TempData.Get<string>("DeleteSuccess") ?? string.Empty;             
                Msd2SummarySearchResultList = _msd2SubmissionSearch.GenerateMsd2ViewModelFilteredOnSearchCriteria(SearchDataInputModel, false);

            }
            catch (NullReferenceException)
            {
                SearchDataInputModel = new Msd2SearchInputModel();
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
                        SearchDataInputModel = new Msd2SearchInputModel();
                    }
                }

                Msd2SummarySearchResultList =  _msd2SubmissionSearch.GenerateMsd2ViewModelFilteredOnSearchCriteria(SearchDataInputModel, false);
                await PopulateDropDowns();
                if (Msd2SummarySearchResultList.Count == 0)
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
        
         public IActionResult OnGetExport(Msd2SearchInputModel searchInput)
        {
            var msd2SummarySearchResultList = _msd2SubmissionSearch.GenerateMsd2ViewModelFilteredOnSearchCriteria( searchInput, true);                    
            var byteArrayCsvData = _csvExtractor.GenerateMsd2CsvExtract(msd2SummarySearchResultList);
            var csvDownloadFileName = "MSD2SearchResults" + DateTime.Now.ToShortDateString() + ".csv";
            return File(byteArrayCsvData, "text/csv", csvDownloadFileName);
        }

        public IActionResult OnGetEditSubmission(int id)
        {
            if (TempData.NotNullOrEmpty() && TempData.Peek(FindSubKey) != null)
            {
                SearchDataInputModel = TempData.Get<Msd2SearchInputModel>(FindSubKey);
                TempData.Keep(FindSubKey);
            }
            TempData.Put("Msd2", _context.Msd2.FirstOrDefault(x => x.Id == id));
            return RedirectToPage("/MSD2/Edit", new { area = "Admin" });
           
                     
        }

        public IActionResult OnGetDeleteSubmission(int id)
        {
            if (TempData.NotNullOrEmpty() && TempData.Peek(FindSubKey) != null)
            {
                SearchDataInputModel = TempData.Get<Msd2SearchInputModel>(FindSubKey);
                TempData.Keep(FindSubKey);
            }
            TempData.Put("Msd2", _context.Msd2.FirstOrDefault(x => x.Id == id));
            return RedirectToPage("/MSD2/Delete", new { area = "Admin" });
        }

        #region privateMethod
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
        #endregion
    }
}
