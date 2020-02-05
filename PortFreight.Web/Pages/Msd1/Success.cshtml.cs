using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Pages.Msd1
{

    public class SuccessModel : BaseMsd1PageModel
    {
        public MSD1 MSD1 { get; set; }

        [BindProperty]
        public string Msd1Id { get; set; }

        public IActionResult OnGet()
        {
            MSD1 = new MSD1(TempData.Get<MSD1>(MSD1Key));
            Msd1Id = MSD1.Msd1Id;
            TempData.Put(MSD1Success, MSD1);

            return Page();
        }

        public IActionResult OnPostVoyageDetails()
        {
            MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Success));
            MSD1.Msd1Id = null;
            MSD1.CargoSummary.Clear();
            MSD1.IsInbound = null;
            MSD1.ReportingPort = string.Empty;
            MSD1.AssociatedPort = string.Empty;
            MSD1.NumVoyages = 1;
            MSD1.RecordRef = null;
            TempData.Put(MSD1Success, MSD1);

            return RedirectToPage("./VoyageDetails");
        }
    }
}