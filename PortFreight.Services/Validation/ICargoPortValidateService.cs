using PortFreight.Data.Entities;
using PortFreight.Services.Validation;
using PortFreight.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static PortFreight.Services.CargoPortValidateService;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PortFreight.Services
{
    public interface ICargoPortValidateService
    {
        bool IsValidPortForCategory(int categoryCode, string reportingPort, string associatedPort, bool? InBound);
        bool IsValidCategoryForPort(string category, string locode);
        (double?, double?, bool, bool) HasTonnagePerUnitExceeded(int categoryCode, uint? totalUnits, uint? UnitsWitCargo, uint? UnitsWithoutCargo, double? grossweight);
        bool IsValidVesselCargo(byte cargoCategory, uint imo);
        ModelStateDictionary ManualModelValidation(MSD1_1 localMSD1, string cargoDescription, CargoItem cargoItem);
        ModelStateDictionary ManualModelCargoValidation(Msd1Data localMSD1, Msd1CargoSummary cargoItem);
        bool IsMandatoryCargoDescription(int categoryCode);
    }
}