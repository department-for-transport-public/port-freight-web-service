using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortFreight.Data.Entities;
using PortFreight.Services;
using PortFreight.Services.FileProcess;
using PortFreight.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortFreight.FileProcess.Common
{
    public class ValidateMsdData
    {
        private ICargoPortValidateService _cargoPortValidateService;
        private IFileProcessService _fileProcessService;
        private FileOptions _settings;
        private ILogger _logger;
        private StringBuilder sb;

        public ValidateMsdData(ICargoPortValidateService cargoPortValidateService,
                               IFileProcessService fileProcessService,
                               IOptions<FileOptions> settings,
                               ILogger<ValidateMsdData> logger)
        {
            _cargoPortValidateService = cargoPortValidateService;
            _fileProcessService = fileProcessService;
            _settings = settings.Value;
            _logger = logger;
        }

        public (bool, string) ValidateMSd1CargoSummary(Msd1Data msd1Data, Msd1CargoSummary cargoItem)
        {
            sb = new StringBuilder();
            var modelState = _cargoPortValidateService.ManualModelCargoValidation(msd1Data, cargoItem);
            if (modelState.IsValid) return (true, string.Empty);
            foreach (var error in modelState.Values.SelectMany(item => item.Errors))
            {
                sb.AppendLine().AppendLine(error.ErrorMessage);
            }

            return (false, sb.ToString());
        }

        public string GetUserName(FlatFile fileInfo)
        {
            var userName = _fileProcessService.GetUserByFileName(fileInfo.FileName);
            if (!string.IsNullOrEmpty(userName)) return userName;
            userName = _fileProcessService.GetAllUsers(fileInfo.SenderId).FirstOrDefault();
            userName = string.IsNullOrEmpty(userName) ? _settings.MaritimeHelpdeskEmailAddress : userName;

            return userName;
        }

        public string ValidateMSD2InwardOutwadCargoWeight(Msd2 msd2Data)
        {
            var validateInwardAndOutwardGrossWeight = string.Empty;
            if (msd2Data != null)
            {
                validateInwardAndOutwardGrossWeight = (msd2Data.GrossWeightInward == 0 && msd2Data.GrossWeightOutward == 0) ?
                    "Inward and Outward Gross weight both can't be 0.Enter a valid gross weight or contact the helpdesk for advice." : string.Empty;
            }
            return validateInwardAndOutwardGrossWeight;
        }

        public string ValidateYearAndQtrSubmission(uint year, UInt16 qtr)
        {
            var currentYear = DateTime.Now.Year;
            var isValid = year <= currentYear;

            if (!isValid)
                return "Enter a valid year/quarter for submission or contact the helpdesk for advice.";
            if (year < currentYear)
                return string.Empty;
            var currentQtr = GetQuarter();
            isValid = qtr <= currentQtr;
            return isValid ? string.Empty : "Enter a valid year/quarter for submission or contact the helpdesk for advice.";
        }

        public int GetQuarter()
        {
            var date = DateTime.Now;
            if (date.Month >= 1 && date.Month <= 3)
                return 1;
            if (date.Month >= 4 && date.Month <= 6)
                return 2;
            if (date.Month >= 7 && date.Month <= 9)
                return 3;
            return 4;
        }
    }
}
