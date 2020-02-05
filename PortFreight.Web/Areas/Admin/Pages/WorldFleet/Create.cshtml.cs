using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using PortFreight.Data;
using PortFreight.Data.Entities;


namespace PortFreight.Web.Areas.Admin.Pages.WorldFleet
{
    public class CreateModel : PageModel
    {
        private readonly PortFreight.Data.PortFreightContext _context;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public Data.Entities.WorldFleet WorldFleet { get; set; }

        public List<SelectListItem> ShipTypeCodeList {get; set;}


        public CreateModel(PortFreightContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            PopulateDropDowns();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns();

                return Page();
            }

            try
            {
                _context.WorldFleet.Add(WorldFleet);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", e.InnerException.Message);

                _logger.LogError(e.Message);

                PopulateDropDowns();

                return Page();
            }
            

            return RedirectToPage("./Index", new { message = "Vessel added" });
        }

        private void PopulateDropDowns()
        {
            CreateShipTypeCodeList();
        }



        private void CreateShipTypeCodeList()
        {
            ShipTypeCodeList = _context.ShipCargoCategory
                .GroupBy(t => t.ShipTypeCode)
                .Select(g => g.First())
                .Select(n => new SelectListItem
                {
                    Value = n.ShipTypeCode.ToString(),
                    Text = n.ShipTypeCode.ToString()
                })
                .ToList();
        }
    }
}