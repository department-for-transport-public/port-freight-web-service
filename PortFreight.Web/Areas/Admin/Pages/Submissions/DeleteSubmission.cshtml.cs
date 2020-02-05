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
    public class DeleteSubmissionModel : PageModel
    {
        private readonly PortFreightContext _context;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly ILogger<EditSubmissionModel> _logger;

        public Msd1Data msd1;

        [BindProperty]
        public uint Imo { get; set; }
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
        [BindProperty]
        public string Id { get; set; }
        public string SuccessMessage { get; set; }

        public DeleteSubmissionModel(PortFreightContext context,
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

         public async Task<IActionResult> OnPostAsync(string id)
        {
            try
            {
                Msd1Data ToRemove = _context.Msd1Data.FirstOrDefault(x => x.Msd1Id == id);

                _context.Msd1Data.Remove(ToRemove);

                _context.SaveChanges();
            
                TempData.Put("DeleteSuccess", "Submission has been succesfully deleted");
            
                return RedirectToPage("/Submissions/FindSubmission", new { area = "Admin" });
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", "There has been an error connecting to the database");
                _logger.LogError(e.Message);
            }

            return Page();
            
        }
       


        public void mapInputsToMSDI(PortFreightUser user)
        {
            msd1 = TempData.GetKeep<Msd1Data>("Msd1");


            msd1.Msd1Id = Id;
            msd1.Imo = Imo;
            msd1.Year = Year;
            msd1.Quarter = Quarter;
            msd1.NumVoyages = NumVoyages;
            msd1.ReportingPort = ReportingPort;
            msd1.AssociatedPort = AssociatedPort;
            msd1.LastUpdatedBy = user.Email;
            msd1.ModifiedDate = DateTime.Now;
            msd1.IsInbound = IsInbound;
        }

       
    }
}