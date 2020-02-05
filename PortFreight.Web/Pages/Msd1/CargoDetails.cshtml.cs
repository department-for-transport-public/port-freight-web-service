using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PortFreight.Web.Pages.Msd1
{

    public class CargoDetailsModel : BaseMsd1PageModel
    {
        [BindProperty]
        public CargoItem CargoItem { get; set; }
        public MSD1 MSD1 { get; set; }
        public bool IsEditMode { get; set; } = false;
        public string TextInputDisplayDriver { get; set; } = string.Empty;
        public List<SelectListItem> CargoGroup { get; set; }
        public List<CargoCategory> CargoCategoriesForCargoGroup { get; set; }
        [BindProperty]
        public bool FromSummary { get; set; }
        private static double DeadweightErrorMargin = 1000.000;
        private readonly string CargoGroupKey = "CargoGroupKey";
        private readonly string CargoCategoryKey = "CargoCategoryKey";
        private readonly string WeightConfigure = "Weight";
        private readonly string CargoConfigure = "Cargo";
        private readonly string NoCargoConfigure = "NoCargo";
        private readonly PortFreightContext _context;
        private readonly ICargoPortValidateService _cargoPortValidateService;

        public CargoDetailsModel(
            PortFreightContext context,
            ICargoPortValidateService cargoPortValidateService)
        {
            _context = context;
            _cargoPortValidateService = cargoPortValidateService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            CargoGroup = await CreateCargoGroupListAsync();
            SetupCargoCategories(0);
            MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));
            FromSummary = Helpers.ReturnBoolFromQueryString(HttpContext, "FromSummary");
            TempData.Put("FromSummary", FromSummary.ToString());
            AddViewBag();
            return Page();
        }

        public async Task<IActionResult> OnPostContinue()
        {
            MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));
            int totalSummaryItems = MSD1.CargoSummary.Count;
            FromSummary = Boolean.TryParse(TempData.Get<string>("FromSummary"), out bool result) ? result : false;

            if (CargoItem.Group == null || CargoItem.Category == null)
            {
                byte groupCode = 0;
                if (CargoItem.Group == null)
                {
                    if (totalSummaryItems > 0)
                    {
                        if (FromSummary)
                        {
                            return RedirectToPage("./SubmitReturn", new { FromSummary = "true", IsEdited = "true" });
                        }
                        return RedirectToPage("./SubmitReturn");
                    }

                    ModelState.AddModelError("CargoItem.Group", "Select a Cargo Group");
                }
                else
                {
                    ModelState.AddModelError("CargoItem.Category", "Select a Cargo Category");
                    IsEditMode = true;
                    groupCode = TempData.GetKeep<CargoGroup>(CargoGroupKey).GroupCode;
                }
                CargoGroup = await CreateCargoGroupListAsync();
                SetupCargoCategories(groupCode);
                AddViewBag();
                return Page();
            }
            var currentCargoGroup = TempData.GetKeep<CargoGroup>(CargoGroupKey);
            var currentCargoCategory = TempData.GetKeep<List<CargoCategory>>(CargoCategoryKey).FirstOrDefault(x => x.Description == CargoItem.Category);
            var localMSD1 = new MSD1(TempData.Get<MSD1>(MSD1Key));
            CargoItemValidation(localMSD1, currentCargoCategory, currentCargoGroup);

            if (!ModelState.IsValid)
            {
                IsEditMode = true;
                CargoGroup = await CreateCargoGroupListAsync();
                SetupCargoCategories(currentCargoGroup.GroupCode);
                TextInputDisplayDriver = buildTextInputDisplayDriver(currentCargoCategory, currentCargoGroup.IsUnitised);
                AddViewBag();
                return Page();
            }

            if (!CargoItem.TotalUnits.HasValue || CargoItem.TotalUnits == 0)
            {
                CargoItem.TotalUnits = (CargoItem.UnitsWithCargo ?? 0) + (CargoItem.UnitsWithoutCargo ?? 0);
            }

            CargoItem.Id = Guid.NewGuid();
            localMSD1.CargoSummary.Add(CargoItem);
            TempData.Put(MSD1Key, localMSD1);

            TempData.Remove(CargoGroupKey);
            TempData.Remove(CargoCategoryKey);

            if (FromSummary)
            {
                return RedirectToPage("./SubmitReturn", new { FromSummary = "true", IsEdited = "true" });
            }
            return RedirectToPage("./SubmitReturn");
        }

        public async Task<IActionResult> OnPostAddCargo()
        {
            MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));

            if (CargoItem.Group == null || CargoItem.Category == null)
            {
                byte groupCode = 0;
                if (string.IsNullOrEmpty(CargoItem.Group))
                {
                    ModelState.AddModelError("CargoItem.Group", "Select a Cargo Group");
                }
                else
                {
                    ModelState.AddModelError("CargoItem.Category", "Select a Cargo Category");
                    IsEditMode = true;
                    groupCode = TempData.GetKeep<CargoGroup>(CargoGroupKey).GroupCode;
                }
                CargoGroup = await CreateCargoGroupListAsync();
                SetupCargoCategories(groupCode);
                MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));
                AddViewBag();

                return Page();
            }

            var currentCargoGroup = TempData.GetKeep<CargoGroup>(CargoGroupKey);
            var currentCargoCategory = TempData.GetKeep<List<CargoCategory>>(CargoCategoryKey)
                .FirstOrDefault(x => x.Description == CargoItem.Category);

            var localMSD1 = new MSD1(TempData.Get<MSD1>(MSD1Key));
            CargoItemValidation(localMSD1, currentCargoCategory, currentCargoGroup);

            if (!ModelState.IsValid)
            {
                IsEditMode = true;
                CargoGroup = await CreateCargoGroupListAsync();
                SetupCargoCategories(currentCargoGroup.GroupCode);
                MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));
                TextInputDisplayDriver = buildTextInputDisplayDriver(currentCargoCategory, currentCargoGroup.IsUnitised);
                AddViewBag();
                return Page();
            }

            if (!CargoItem.TotalUnits.HasValue || CargoItem.TotalUnits == 0)
            {
                CargoItem.TotalUnits = (CargoItem.UnitsWithCargo ?? 0) + (CargoItem.UnitsWithoutCargo ?? 0);
            }


            CargoItem.Id = Guid.NewGuid();
            localMSD1.CargoSummary.Add(CargoItem);
            TempData.Put(MSD1Key, localMSD1);

            string fromSummary = TempData.Get<string>("FromSummary") ?? "false";
            return RedirectToPage("CargoDetails", new { FromSummary = fromSummary });
        }

        public IActionResult OnPostRemoveCargo(Guid? Id)
        {
            if (Id != null && Id != Guid.Empty)
            {
                var localMSD1 = new MSD1(TempData.Get<MSD1>(MSD1Key));
                var cargoItem = localMSD1.CargoSummary.Where(x => x.Id == Id).SingleOrDefault();
                localMSD1.CargoSummary.Remove(cargoItem);
                TempData.Put(MSD1Key, localMSD1);
            }
            string fromSummary = TempData.Get<string>("FromSummary") ?? "false";
            return RedirectToPage("CargoDetails", new { FromSummary = fromSummary });
        }

        public JsonResult OnGetCargoCategories(string input)
        {
            var cargoGroup = _context.CargoGroup.FirstOrDefault(x => x.Description == input);

            var cargoCategory = _context.CargoCategory.Where(x => x.GroupCode == cargoGroup.GroupCode);
            SetupCargoCategories(cargoGroup.GroupCode);

            var result = cargoCategory.Select(x => new
            {
                Value = x.Description,
                Text = x.Description,
            });

            TempData.Put(CargoCategoryKey, cargoCategory);

            TempData.Put(CargoGroupKey, cargoGroup);

            return new JsonResult(result);
        }

        public JsonResult OnGetUiFormat(string input)
        {
            var cargoGroup = TempData.GetKeep<CargoGroup>(CargoGroupKey);
            var cargoCategories = TempData.GetKeep<List<CargoCategory>>(CargoCategoryKey);
            var singleCategory = cargoCategories.FirstOrDefault(x => x.Description == input);
            var result = buildTextInputDisplayDriver(singleCategory, cargoGroup.IsUnitised);

            return new JsonResult(result);
        }

        public string buildTextInputDisplayDriver(CargoCategory x, bool Unitised)
        {
            string result = string.Empty;

            if (Unitised)
            {
                if (x.TakesCargo)
                {
                    result = CargoConfigure;
                }
                else
                {
                    result = NoCargoConfigure;
                }
            }

            if (x.HasWeight)
            {
                result += WeightConfigure;
            }

            return result;
        }

        private void SetupCargoCategories(byte groupCode)
        {
            if (IsEditMode)
            {
                CargoCategoriesForCargoGroup = _context.CargoCategory.Where(x => x.GroupCode == groupCode).ToList();
            }
            else
            {
                CargoCategoriesForCargoGroup = new List<CargoCategory>() { new CargoCategory() { Description = "--Disabled--" } };
            }
        }

        private void ManualModelValidation(MSD1 localMSD1)
        {
            var currentCargoGroup = TempData.GetKeep<CargoGroup>(CargoGroupKey);
            var currentCargoCategory = TempData.GetKeep<List<CargoCategory>>(CargoCategoryKey).FirstOrDefault(x => x.Description == CargoItem.Category);

            TextInputDisplayDriver = buildTextInputDisplayDriver(currentCargoCategory, currentCargoGroup.IsUnitised);

            var grossWeightVisible = TextInputDisplayDriver.Contains(WeightConfigure);
            var numUnitsVisible = TextInputDisplayDriver.Contains(NoCargoConfigure);
            var numUnitsWithCargoVisible = TextInputDisplayDriver.Contains(CargoConfigure) && !numUnitsVisible;
            var numUnitsWithoutCargoVisible = numUnitsWithCargoVisible;

            if (numUnitsVisible && (!CargoItem.TotalUnits.HasValue || CargoItem.TotalUnits == 0))
            {
                ModelState.AddModelError("CargoItem.TotalUnits", "Enter number of units");
            }

            if (numUnitsWithCargoVisible && (!CargoItem.UnitsWithCargo.HasValue))
            {
                ModelState.AddModelError("CargoItem.UnitsWithCargo", "Enter number of units carrying cargo");
            }

            if (numUnitsWithoutCargoVisible && (!CargoItem.UnitsWithoutCargo.HasValue))
            {
                ModelState.AddModelError("CargoItem.UnitsWithoutCargo", "Enter number of units without cargo");
            }

            if (grossWeightVisible)
            {
                if (!CargoItem.GrossWeight.HasValue)
                {
                    ModelState.AddModelError("CargoItem.GrossWeight", "Enter the weight. Type 0 if no weight available");
                }
                else
                {
                    if (!numUnitsVisible && !numUnitsWithCargoVisible && !numUnitsWithoutCargoVisible && CargoItem.GrossWeight == 0)
                    {
                        ModelState.AddModelError("CargoItem.GrossWeight", "Enter weight greater than 0");
                    }
                    else if (numUnitsVisible && !numUnitsWithCargoVisible && !numUnitsWithoutCargoVisible && CargoItem.GrossWeight == 0)
                    {
                        ModelState.AddModelError("CargoItem.GrossWeight", "Enter weight greater than 0");
                    }
                    else
                    {
                        var totalGrossWeight = localMSD1.CargoSummary.Sum(x => x.GrossWeight) + CargoItem.GrossWeight;
                        if ((CargoItem.GrossWeight - DeadweightErrorMargin > localMSD1.Deadweight * localMSD1.NumVoyages)
                             || (totalGrossWeight - DeadweightErrorMargin > localMSD1.Deadweight * localMSD1.NumVoyages))
                        {
                            ModelState.AddModelError("CargoItem.GrossWeight",
                                        totalGrossWeight + " exceeds the deadweight of this ship. If deadweight of the ship is incorrect, contact the helpdesk");
                        }
                    }
                }
            }

            if (numUnitsWithCargoVisible &&
                numUnitsWithoutCargoVisible &&
                CargoItem.UnitsWithCargo == 0 &&
                CargoItem.UnitsWithoutCargo == 0)
            {
                ModelState.AddModelError("CargoItem.UnitsWithCargo", "Number of units with and without cargo cannot both be 0");
                ModelState.AddModelError("CargoItem.UnitsWithoutCargo", "Number of units with and without cargo cannot both be 0");
            }

            if (numUnitsWithCargoVisible && CargoItem.UnitsWithCargo > 0 && CargoItem.GrossWeight == 0)
            {
                ModelState.AddModelError("CargoItem.GrossWeight",
                            "You entered " + CargoItem.UnitsWithCargo + " units with cargo. Enter the weight of this cargo");
            }

            if (numUnitsWithCargoVisible && CargoItem.UnitsWithCargo == 0 && CargoItem.GrossWeight > 0)
            {
                ModelState.AddModelError("CargoItem.UnitsWithCargo",
                            "You entered weight as " + CargoItem.GrossWeight + " tonnes. Enter number of units that were carrying this cargo");
            }
        }

        private void CargoCategoryHardValidations(MSD1 localMSD1, CargoCategory currentCargoCategory)
        {
            bool isvalid = _cargoPortValidateService.IsValidCategoryForPort(CargoItem.Category, localMSD1.AssociatedPort);
            if (!isvalid)
            {
                ModelState.AddModelError("CargoItem.Category", $" Cargo Category cannot be carried through " + "'" + localMSD1.AssociatedPort + "'");
            }

            var (allowedTonnage, avgWeightPerUnit, isTonnageExceeded, IsUnitsWithCargoZero) = _cargoPortValidateService.HasTonnagePerUnitExceeded(currentCargoCategory.CategoryCode, CargoItem.TotalUnits, CargoItem.UnitsWithCargo, CargoItem.UnitsWithoutCargo, CargoItem.GrossWeight);
            if (isTonnageExceeded)
            {
                ModelState.AddModelError("CargoItem.GrossWeight", $" Average gross weight entered '" + avgWeightPerUnit + " tonnes' per unit has exceeded maximum permitted '" + allowedTonnage + " tonnes' for Cargo Category: " + "'" + CargoItem.Category + "'");
            }
            if (!IsUnitsWithCargoZero)
            {
                ModelState.AddModelError("CargoItem.", $" The number of units without cargo must be '0' for selected " + CargoItem.Category);
            }

            bool RequiredCargoDescription = _cargoPortValidateService.IsMandatoryCargoDescription(currentCargoCategory.CategoryCode);
            if (RequiredCargoDescription && CargoItem.Description == null)
            {
                ModelState.AddModelError("CargoItem.Description", $"The Cargo description is required");
            }
        }

        private async Task<List<SelectListItem>> CreateCargoGroupListAsync()
        {
            return CargoGroup = await _context.CargoGroup.
                Select(n => new SelectListItem
                {
                    Value = n.Description,
                    Text = n.Description
                }).ToListAsync();
        }

        private void CargoItemValidation(MSD1 localMSD1, CargoCategory currentCargoCategory, CargoGroup currentCargoGroup)
        {
            ManualModelValidation(localMSD1);

            CargoCategoryHardValidations(localMSD1, currentCargoCategory);
            bool selectedCategory12 = currentCargoCategory.CategoryCode == (int)PortCategory.CrudeOilCode;
            bool selectedCategory13 = currentCargoCategory.CategoryCode == (int)PortCategory.OilProductsCode;

            bool isValidCategory = _cargoPortValidateService.IsValidPortForCategory(currentCargoCategory.CategoryCode, localMSD1.ReportingPort, localMSD1.AssociatedPort, localMSD1.IsInbound);

            if (!isValidCategory)
            {
                if (selectedCategory12)
                {
                    ModelState.AddModelError("CargoItem.Category", $"Crude Oil cannot be carried through " + localMSD1.ReportingPort);
                }
                if (selectedCategory13)
                {
                    var portName = (localMSD1.IsInbound == true ? localMSD1.AssociatedPort : localMSD1.ReportingPort);
                    ModelState.AddModelError("CargoItem.Category", $"Oil Products cannot be carried through " + portName);
                }
            }

            bool isValidCategoryVessel = _cargoPortValidateService.IsValidVesselCargo(currentCargoCategory.CategoryCode, localMSD1.Imo);
            if (!isValidCategoryVessel)
            {
                ModelState.AddModelError("CargoItem.Category", $"Category '" + currentCargoCategory.Description + "' is not valid for the vessel '" + localMSD1.Imo + "-" + localMSD1.ShipName + "'");
            }
        }

        private void AddViewBag()
        {
            if (MSD1.CargoSummary.Count > 0)
            {
                ViewBag.AddItem = "Add another cargo";
            }
            else
            {
                ViewBag.AddItem = "Add cargo";
            }
        }

        private void MapVMtoServiceModel(MSD1 localMSD1)
        {
            var serviceLocalMSD1 = new Services.Models.MSD1_1
            {
                Imo = localMSD1.Imo,
                ShipName = localMSD1.ShipName,
                DeadWeight = localMSD1.Deadweight,
                Year = localMSD1.Year,
                Quarter = localMSD1.Quarter,
                AssociatedPort = localMSD1.AssociatedPort,
                ReportingPort = localMSD1.ReportingPort,
                NumVoyages = localMSD1.NumVoyages,
                IsInbound = localMSD1.IsInbound,
                Msd1Id = localMSD1.Msd1Id,
                AgentSenderID = localMSD1.AgentSenderID,
                AgentCompanyName = localMSD1.AgentCompanyName,
                LineSenderID = localMSD1.LineSenderID,
                LineCompanyName = localMSD1.LineCompanyName,
                FlagCode = localMSD1.FlagCode,
                RecordRef = localMSD1.RecordRef
            };
            var serviceCargoItem = new Services.Models.CargoItem
            {
                Id = CargoItem.Id,
                Category = CargoItem.Category,
                Group = CargoItem.Group,
                UnitsWithCargo = CargoItem.UnitsWithCargo,
                UnitsWithoutCargo = CargoItem.UnitsWithoutCargo,
                TotalUnits = CargoItem.TotalUnits,
                GrossWeight = CargoItem.GrossWeight,
                Description = CargoItem.Description
            };
        }
    }
}