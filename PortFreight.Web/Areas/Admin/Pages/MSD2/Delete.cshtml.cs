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
    public class DeleteModel : PageModel
    {
        private readonly PortFreightContext _context;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly ILogger<EditSubmissionModel> _logger;

        public Msd2 msd2;

        [BindProperty]
        public string SenderId { get; set; }
        [BindProperty]
        public uint Year { get; set; }
        [BindProperty]
        public ushort Quarter { get; set; }
        [BindProperty]
        public string ReportingPort { get; set; }
        [BindProperty]
        public int Id { get; set; }
        [BindProperty]
        public DateTime CreatedDate { get; set; }
        [BindProperty]
        public DateTime ModifiedDate { get; set; }
        [BindProperty]
        public string LastUpdatedBy { get; set; }


        public string SuccessMessage { get; set; }


        public DeleteModel(PortFreightContext context,
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
                msd2 = TempData.GetKeep<Msd2>("Msd2");
            }
            catch
            {
                return Page();
            }

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            try
            {
                Msd2 ToRemove = _context.Msd2.FirstOrDefault(x => x.Id == id);

                _context.Msd2.Remove(ToRemove);

                _context.SaveChanges();

                TempData.Put("DeleteSuccess", "Submission has been succesfully deleted");

                return RedirectToPage("./Index", new { area = "Admin" });
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", "There has been an error connecting to the database");
                _logger.LogError(e.Message);
            }

            return Page();

        }


    }

       
   
      

    }
