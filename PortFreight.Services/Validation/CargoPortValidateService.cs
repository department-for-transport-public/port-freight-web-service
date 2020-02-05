using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Services.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PortFreight.Services
{
    public enum CargoCategory1
    {
        [Description("None")]
        None = 0,
        [Description("Liquefied Gas")]
        LiquefiedGas = 11,
        [Description("Crude Oil")]
        CrudeOil = 12,
        [Description("Oil Products")]
        OilProducts = 13,
        [Description("Other Liquid Bulk Products")]
        OtherLiquidBulkProducts = 19,
        [Description("Other Dry Bulk")]
        OtherDryBulk = 29,
        [Description("Iron and Steel Products")]
        IronAndSteelProducts = 92,
        [Description("Other General Cargo (incl Containers < 20')")]
        OtherGeneralCargoInclContainersLessThan20 = 99
    }

    public enum CargoCategory2
    {
        [Description("None")]
        None = 0,
        [Description("Ores including scrap")]
        Oresincludingscrap = 21,
        [Description("Coal")]
        Coal = 22,
        [Description("Iron and Steel Products")]
        IronandSteelProducts = 92
    }

    public enum CategoryCodeForDescription
    {
        [Description("Other Liquid Bulk Products")]
        OtherLiquidBulkProducts = 19,
        [Description("Agricultural Products (e.g. grain, soya, tapioca)")]
        AgriculturalProducts = 23,
        [Description("Other Dry Bulk")]
        OtherDryBulk = 29,
        [Description("Forestry Products")]
        ForestryProducts = 91,
        [Description("Iron and Steel Products")]
        IronandSteelProducts = 92,
        [Description("Other General Cargo (incl Containers < 20')")]
        OtherGeneralCargoInclContainersLessThan20 = 99
    }

    public class CargoPortValidateService : ICargoPortValidateService
    {
        private static double allowedTonnageHeavyCow = 0.5;
        private readonly PortFreightContext _portFreightContext;
        private readonly IHelperService _helperService;
        private readonly string CargoGroupKey = "CargoGroupKey";
        private readonly string CargoCategoryKey = "CargoCategoryKey";
        private readonly string WeightConfigure = "Weight";
        private readonly string CargoConfigure = "Cargo";
        private readonly string NoCargoConfigure = "NoCargo";
        private decimal totalGrossWeight;
        public string TextInputDisplayDriver { get; set; } = string.Empty;
        private ModelStateDictionary _modelState;

        public CargoPortValidateService(PortFreightContext portFreightContext, IHelperService helperService)
        {
            _modelState = new ModelStateDictionary();
            _portFreightContext = portFreightContext;
            _helperService = helperService;
        }

        public bool IsMandatoryCargoDescription(int categoryCode)
        {
            return Enum.IsDefined(typeof(CategoryCodeForDescription), categoryCode);
        }

        public bool IsValidPortForCategory(int categoryCode, string reportingPort, string associatedPort, bool? InBound)
        {
            var allowedCat = true;
            switch (categoryCode)
            {
                case 12:
                    {
                        allowedCat = _portFreightContext.PortCargoOil.AsQueryable().AsNoTracking()
                           .Where(a => a.Locode == _helperService.GetPortCodeByPort(reportingPort))
                           .Select(a => a.AllowCategory12).SingleOrDefault();
                    }
                    break;
                case 13:
                    {
                        if (InBound == true)
                        {
                            var gbPort = _helperService.GetCountryCodeByPort(associatedPort);
                            allowedCat = gbPort != null && gbPort.Equals("GB") ? IsAllowedPort(associatedPort) : true;
                        }
                        else
                        {
                            allowedCat = IsAllowedPort(reportingPort);
                        }
                    }
                    break;
                default:
                    {
                    }
                    break;
            }
            return allowedCat;
        }

        public (double?, double?, bool, bool) HasTonnagePerUnitExceeded(int categoryCode, uint? totalUnits, uint? UnitsWithCargo, uint? UnitsWithoutCargo, double? grossweight)
        {

            var IsTonnageExceeded = false;
            var IsUnitsWithCargoZero56 = true;
            double? allowedTonnage = _portFreightContext.CargoCategory.AsQueryable().AsNoTracking()
                                         .Where(a => a.CategoryCode == categoryCode && a.MaxWeight.HasValue)
                                         .Select(a => a.MaxWeight).SingleOrDefault();

            double? avgWeightPerUnit = allowedTonnage;
            if (grossweight > 0)
            {
                switch (categoryCode)
                {
                    case 31:
                    case 32:
                    case 33:
                    case 34:
                    case 51:
                    case 61:
                        {
                            if (UnitsWithCargo > 0)
                                avgWeightPerUnit = (double)grossweight / UnitsWithCargo;
                            IsTonnageExceeded = allowedTonnage < avgWeightPerUnit;
                        }
                        break;
                    case 52:
                    case 53:
                    case 54:
                    case 62:
                        {
                            if (UnitsWithoutCargo > 0)
                                IsUnitsWithCargoZero56 = false;
                        }
                        break;
                    case 56:
                        {
                            if (totalUnits > 0)
                            {
                                avgWeightPerUnit = (double)grossweight / totalUnits;
                                IsTonnageExceeded = allowedTonnageHeavyCow < Math.Round(avgWeightPerUnit.Value, 2);
                                allowedTonnage = allowedTonnageHeavyCow;
                            }
                            if (UnitsWithoutCargo > 0)
                                IsUnitsWithCargoZero56 = false;
                        }
                        break;
                    default:
                        {
                        }
                        break;
                }
            }
            return (allowedTonnage, avgWeightPerUnit, IsTonnageExceeded, IsUnitsWithCargoZero56);
        }

        public bool IsValidVesselCargo(byte cargoCategory, uint imo)
        {
            bool allowedCategoryCode = false;
            var shipTypeCode = _portFreightContext.WorldFleet.AsQueryable().AsNoTracking()
                            .Where(a => a.Imo == imo)
                            .Select(a => a.ShipTypeCode).SingleOrDefault();
            if (shipTypeCode == 0)
            { allowedCategoryCode = true; }
            else
            {
                allowedCategoryCode = _portFreightContext.ShipCargoCategory.AsQueryable().AsNoTracking()
                               .Where(a => a.ShipTypeCode == shipTypeCode && a.CargoCategoryCode == cargoCategory).Any();
            }

            return allowedCategoryCode;
        }

        public bool IsValidCategoryForPort(string cargoCategory, string associatedPort)
        {
            var allowedCat = true;
            var locode = _helperService.GetPortCodeByPort(associatedPort);
            switch (locode)
            {
                case "ZZOF1":
                case "ZZOF2":
                    {
                        allowedCat = (GetValueFromDescription<CargoCategory1>(cargoCategory).ToString() != "None");
                    }
                    break;
                case "ZZAG1":
                case "ZZAG2":
                case "GBGSA":
                case "GBLLD":
                    {
                        allowedCat = GetDescription(CargoCategory1.OtherDryBulk).ToString() == cargoCategory;
                    }
                    break;
                case "GBPTB":
                    {
                        allowedCat = (GetValueFromDescription<CargoCategory2>(cargoCategory).ToString() == "None");
                    }
                    break;
                default:
                    {
                    }
                    break;
            };

            return allowedCat;
        }

        public ModelStateDictionary ManualModelValidation(MSD1_1 localMSD1, string cargoDescription, CargoItem cargoItem)
        {
            var currentCargoGroup = _portFreightContext.CargoGroup.FirstOrDefault(x => x.Description == cargoDescription);

            var cargoCategory = _portFreightContext.CargoCategory.Where(x => x.GroupCode == currentCargoGroup.GroupCode);
            var currentCargoCategory = cargoCategory.FirstOrDefault(x => x.Description == cargoItem.Category);

            TextInputDisplayDriver = BuildTextInputDisplayDriver(currentCargoCategory, currentCargoGroup.IsUnitised);

            var grossWeightVisible = TextInputDisplayDriver.Contains(WeightConfigure);
            var numUnitsVisible = TextInputDisplayDriver.Contains(NoCargoConfigure);
            var numUnitsWithCargoVisible = TextInputDisplayDriver.Contains(CargoConfigure) && !numUnitsVisible;
            var numUnitsWithoutCargoVisible = numUnitsWithCargoVisible;

            if (numUnitsVisible && (!cargoItem.TotalUnits.HasValue || cargoItem.TotalUnits == 0))
            {
                _modelState.AddModelError("CargoItem.TotalUnits", "Enter number of units");
            }

            if (numUnitsWithCargoVisible && (!cargoItem.UnitsWithCargo.HasValue))
            {
                _modelState.AddModelError("CargoItem.UnitsWithCargo", "Enter number of units carrying cargo");
            }

            if (numUnitsWithoutCargoVisible && (!cargoItem.UnitsWithoutCargo.HasValue))
            {
                _modelState.AddModelError("CargoItem.UnitsWithoutCargo", "Enter number of units without cargo");
            }

            if (grossWeightVisible)
            {
                if (!cargoItem.GrossWeight.HasValue)
                {
                    _modelState.AddModelError("CargoItem.GrossWeight", "Enter the weight. Type 0 if no weight available");
                }
                else
                {
                    if (!numUnitsVisible && !numUnitsWithCargoVisible && !numUnitsWithoutCargoVisible && cargoItem.GrossWeight == 0)
                    {
                        _modelState.AddModelError("CargoItem.GrossWeight", "Enter weight greater than 0.");
                    }
                    else if (numUnitsVisible && !numUnitsWithCargoVisible && !numUnitsWithoutCargoVisible && cargoItem.GrossWeight == 0)
                    {
                        _modelState.AddModelError("CargoItem.GrossWeight", "Enter weight greater than 0.");
                    }
                    else
                    {
                        var totalGrossWeight = localMSD1.CargoSummary.Sum(x => x.GrossWeight) + cargoItem.GrossWeight;
                        if (string.IsNullOrEmpty(cargoItem.Description) && ((cargoItem.GrossWeight > (localMSD1.DeadWeight + 1) * localMSD1.NumVoyages)
                             || (totalGrossWeight > (localMSD1.DeadWeight + 1) * localMSD1.NumVoyages)))
                        {
                            _modelState.AddModelError("CargoItem.GrossWeight",
                                        totalGrossWeight + " exceeds the deadweight of this ship. Correct the tonnage or contact the helpdesk.");
                        }
                    }
                }
            }

            if (numUnitsWithCargoVisible &&
                numUnitsWithoutCargoVisible &&
                cargoItem.UnitsWithCargo == 0 &&
                cargoItem.UnitsWithoutCargo == 0)
            {
                _modelState.AddModelError("CargoItem.UnitsWithCargo", "Number of units with and without cargo cannot both be 0. Correct the number of units or contact the helpdesk.");
                _modelState.AddModelError("CargoItem.UnitsWithoutCargo", "Number of units with and without cargo cannot both be 0. Correct the number of units or contact the helpdesk.");
            }

            if (numUnitsWithCargoVisible && cargoItem.UnitsWithCargo > 0 && cargoItem.GrossWeight == 0)
            {
                _modelState.AddModelError("CargoItem.GrossWeight",
                            "You reported " + cargoItem.UnitsWithCargo + " loaded units with cargo but no weight. Enter the total weight of this cargo.");
            }

            if (numUnitsWithCargoVisible && cargoItem.UnitsWithCargo == 0 && cargoItem.GrossWeight > 0)
            {
                _modelState.AddModelError("CargoItem.UnitsWithCargo",
                            "You reported cargo weighing " + cargoItem.GrossWeight + " tonnes but 0 units with cargo. Enter the number of units that were carrying this cargo.");
            }
            return _modelState;
        }

        public ModelStateDictionary ManualModelCargoValidation(Msd1Data localMSD1, Msd1CargoSummary cargoItem)
        {
            totalGrossWeight = 0;

            _modelState = new ModelStateDictionary();

            var currentCargoGroup = _portFreightContext.CargoGroup.
                                      Join(_portFreightContext.CargoCategory, cg => cg.GroupCode, cc => cc.GroupCode,
                                      (cg, cc) => new { cg, cc })
                                      .Where(x => x.cc.CategoryCode.Equals(cargoItem.CategoryCode))
                                      .Select(x => x.cg).FirstOrDefault();

            if (currentCargoGroup == null)
            {
                _modelState.AddModelError("CargoItem.CategoryCode", "Invalid value for category code. Please correct or contact the helpdesk for advice.");
                return _modelState;
            }

            var cargoCategory = _portFreightContext.CargoCategory.Where(x => x.GroupCode == currentCargoGroup.GroupCode);
            var currentCargoCategory = cargoCategory.FirstOrDefault(x => x.CategoryCode == cargoItem.CategoryCode);

            TextInputDisplayDriver = BuildTextInputDisplayDriver(currentCargoCategory, currentCargoGroup.IsUnitised);

            var grossWeightVisible = TextInputDisplayDriver.Contains(WeightConfigure);
            var numUnitsVisible = TextInputDisplayDriver.Contains(NoCargoConfigure);
            var numUnitsWithCargoVisible = TextInputDisplayDriver.Contains(CargoConfigure) && !numUnitsVisible;
            var numUnitsWithoutCargoVisible = numUnitsWithCargoVisible;

            if (numUnitsVisible && (!cargoItem.TotalUnits.HasValue || cargoItem.TotalUnits == 0))
            {
                _modelState.AddModelError("CargoItem.TotalUnits", "Missing value for total units. Please correct or contact the helpdesk for advice.");
            }

            if (numUnitsWithCargoVisible && (!cargoItem.UnitsWithCargo.HasValue))
            {
                _modelState.AddModelError("CargoItem.UnitsWithCargo", "Invalid value for units with cargo. Please correct or contact the helpdesk for advice.");
            }

            if (numUnitsWithoutCargoVisible && (!cargoItem.UnitsWithoutCargo.HasValue))
            {
                _modelState.AddModelError("CargoItem.UnitsWithoutCargo", "Invalid value for units without cargo. Please correct or contact the helpdesk for advice.");
            }

            if (grossWeightVisible)
            {
                if (!cargoItem.GrossWeight.HasValue)
                {
                    _modelState.AddModelError("CargoItem.GrossWeight", "Invalid value for gross weight. Please correct or contact the helpdesk for advice.");
                }
                else if (!numUnitsWithCargoVisible && !numUnitsWithoutCargoVisible && cargoItem.GrossWeight == 0)
                {
                    _modelState.AddModelError("CargoItem.GrossWeight", "Gross weight cannot be 0. Please correct or contact the helpdesk for advice.");
                }
            }

            if (numUnitsWithCargoVisible &&
                numUnitsWithoutCargoVisible &&
                cargoItem.UnitsWithCargo == 0 &&
                cargoItem.UnitsWithoutCargo == 0)
            {
                _modelState.AddModelError("CargoItem.UnitsWithCargo", "Number of units with and without cargo cannot both be 0. Please correct or contact the helpdesk for advice.");
            }

            if (numUnitsWithCargoVisible && cargoItem.UnitsWithCargo > 0 && cargoItem.GrossWeight == 0)
            {
                _modelState.AddModelError("CargoItem.GrossWeight",
                            "You reported " + cargoItem.UnitsWithCargo + " units with cargo but no weight. Enter the weight of this cargo or contact the helpdesk for advice.");
            }

            if (numUnitsWithCargoVisible && cargoItem.UnitsWithCargo == 0 && cargoItem.GrossWeight > 0)
            {
                _modelState.AddModelError("CargoItem.UnitsWithCargo",
                            "You reported cargo weighing " + cargoItem.GrossWeight + " tonnes. Enter the number of units that were carrying this cargo or contact the helpdesk for advice.");
            }
            return _modelState;
        }

        private bool IsAllowedPort(string portName)
        {
            return _portFreightContext.PortCargoOil.AsQueryable().AsNoTracking()
                   .Where(a => a.Locode == _helperService.GetPortCodeByPort(portName))
                   .Select(a => a.AllowCategory13Outward).SingleOrDefault();
        }

        private static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            return default(T);
        }

        private static string GetDescription(Enum value) => value
                    .GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description;

        private string BuildTextInputDisplayDriver(CargoCategory x, bool Unitised)
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
    }
}
