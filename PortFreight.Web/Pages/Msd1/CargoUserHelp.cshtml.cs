using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Pages.Msd1
{
    public class CargoUserHelpModel : BaseMsd1PageModel
    {
        public MSD1 MSD1 { get; set; }
        private readonly PortFreightContext _context;
        public List<CargoGroup> CargoGroup { get; set; }
        public List<CargoCategory> CargoCategoriesForCargoGroup { get; set; }

        public CargoUserHelpModel(PortFreightContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> OnGet()
        {
            MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));
            CargoGroup = await CreateCargoGroupListAsync();
            return Page();
        }
        private async Task<List<CargoGroup>> CreateCargoGroupListAsync()
        {
            return await _context.CargoGroup.Include(c => c.CargoCategory).ToListAsync();
        }
    }
}