using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Web.Utilities;
namespace PortFreight.Web.Pages.Msd1
{
    public class IndexModel : BaseMsd1PageModel
    {
        public void OnGet()
        {
            TempData.Remove(MSD1Success);
            TempData.Remove(MSD1Key);
            TempData.Remove("CargoCategoryKey");
            TempData.Remove("CargoGroupKey");
        }
    }
}