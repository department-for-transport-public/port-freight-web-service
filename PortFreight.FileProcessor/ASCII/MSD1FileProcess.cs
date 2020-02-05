using Microsoft.Extensions.Logging;
using PortFreight.Data.Entities;
using PortFreight.FileProcess.Common;
using PortFreight.Services.Common;
using PortFreight.Services.FileProcess;
using PortFreight.Services.MSD1;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PortFreight.FileProcessor.ASCII
{
    public class MSD1FileProcess
    {
        private readonly ILogger _logger;
        private readonly IFileProcessService _fileProcessService;
        private readonly IMsd1DataService _msd1DataService;
        private IHelperService _helperService;
        private Msd1Data _msd1;
        private Msd1CargoSummary _msd1CargoSummary;
        private FlatFile _fileInfo;
        private ValidateMsdData _validateMsdData;
        private StringBuilder bodyErrorMsg;
        private StringBuilder recordErrors;

        public MSD1FileProcess(Msd1Data msd1,
                              Msd1CargoSummary msd1CargoSummary,
                              FlatFile fileInfo,
                              ValidateMsdData validateMsdData,
                              IHelperService helperService,
                              IFileProcessService fileProcessService,
                              IMsd1DataService msd1DataService,
                              ILogger<MSD1FileProcess> logger

            )
        {
            _msd1 = msd1;
            _msd1CargoSummary = msd1CargoSummary;
            _fileInfo = fileInfo;
            _helperService = helperService;
            _fileProcessService = fileProcessService;
            _msd1DataService = msd1DataService;
            _logger = logger;
            _validateMsdData = validateMsdData;
            recordErrors = new StringBuilder();
            bodyErrorMsg = new StringBuilder();
        }

        public string ProcessMsd1Records(string filename, FlatFile fileInfo)
        {
            _fileInfo = fileInfo;
            string record = string.Empty;
            bodyErrorMsg.Clear();

            var lines = File.ReadAllLines(filename);
            var noMsd1Data = lines.Length < 2 ? "There are no records to process in this file. Please correct it or contact the helpdesk for advice." : string.Empty;
            recordErrors.AppendLine(noMsd1Data).AppendLine();

            for (int i = 1; i < lines.Length; i++)
            {
                record = lines[i];
                int recordMinLength = 59;
                int startIndex = 8;

                try
                {
                    if (!string.IsNullOrEmpty(record.Trim()))
                    {
                        int endIndex = Math.Min(record.Length - startIndex, recordMinLength);
                        int endIndexPrevious = lines[i - 1].Length - startIndex;
                        int endIndexCurrent = record.Length - startIndex;

                        string currentLineLength = endIndexCurrent > startIndex && record.Length >= recordMinLength ? record.Substring(startIndex, endIndex) : string.Empty;
                        if (string.IsNullOrEmpty(currentLineLength))
                        {
                            recordErrors.AppendLine().AppendFormat('"' + record + '"' + " is not a valid record because it is missing some data");
                        }
                        else
                        {
                            string previousLineLength = endIndexPrevious > startIndex && lines[i - 1].Length >= recordMinLength ? lines[i - 1].Substring(startIndex, Math.Min(endIndexPrevious, endIndex)) : string.Empty;
                            if (i == 1 || string.IsNullOrEmpty(lines[i - 1].Trim()) || !currentLineLength.Equals(previousLineLength))
                            {
                                _msd1.Msd1Id = _helperService.GetUniqueKey();
                            }
                            ValidateAndPopulateMsd1DataFromFlatFile(record, filename);

                            if (string.IsNullOrEmpty(recordErrors.ToString().Trim()) && _msd1.Msd1Id != null)
                            {
                                AddCargoSummaryToMsd1Data(record);
                                int endIndexNext = i == (lines.Length - 1) ? 0 : lines[i + 1].Length;
                                string nextLineLength = endIndexNext > startIndex && lines[i + 1].Length >= recordMinLength ? lines[i + 1].Substring(startIndex, endIndex) : string.Empty;
                                if (i == (lines.Length - 1) || !record.Substring(startIndex, endIndex).Equals(nextLineLength))
                                {
                                    AddMsd1DataToDatabase();
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(recordErrors.ToString().Trim()))
                        {
                            bodyErrorMsg.AppendLine(" #Line from your file").AppendLine().AppendLine(record)
                                .AppendLine(" #What is wrong and how to fix it").AppendLine().Append(recordErrors)
                                .AppendLine().Append(" ___").AppendLine();
                            recordErrors.Clear();
                        }
                    }
                }
                catch (Exception err)
                {
                    _logger.LogError("Ascii Record error:", err.Message, err.InnerException != null ? err.InnerException.Message : string.Empty, err.StackTrace);
                    if (_fileInfo != null)
                    {
                        recordErrors.AppendLine(err.Message).Append(" Please correct it or contact the helpdesk for advice.");
                        bodyErrorMsg.AppendLine(" #Line from your file").AppendLine().AppendLine(record)
                                    .AppendLine(" #What is wrong and how to fix it").AppendLine().Append(recordErrors)
                                    .AppendLine().Append(" ___").AppendLine();
                        recordErrors.Clear();
                    }
                }
            }
            return bodyErrorMsg.ToString();
        }

        private bool ValidateAndPopulateMsd1DataFromFlatFile(string record, string filename)
        {
            _msd1.UserName = _validateMsdData.GetUserName(_fileInfo);
            _msd1.DataSourceId = (int)DataSource.ASCII;
            _msd1.RecordRef = record.Substring(0, 8);

            var validateLinSenderId = string.IsNullOrEmpty(record.Substring(8, 6)) || record.Substring(8, 6).Trim().Length < 6
                ? record.Substring(8, 6) + " is not a recognised code for shipping line. Replace it or contact the helpdesk for advice."
                : string.Empty;

            _msd1.LineSenderId = !string.IsNullOrEmpty(validateLinSenderId) ? string.Empty : record.Substring(8, 6);
            if (!string.IsNullOrEmpty(validateLinSenderId))
                recordErrors.AppendLine().AppendFormat(validateLinSenderId);

            var validateAgentSenderId = string.IsNullOrEmpty(record.Substring(14, 6)) || record.Substring(14, 6).Trim().Length < 6
                ? record.Substring(14, 6) + " is not a recognised code for shipping agent. Replace it or contact the helpdesk for advice."
                : string.Empty;

            _msd1.AgentSenderId = !string.IsNullOrEmpty(validateAgentSenderId) ? string.Empty : record.Substring(14, 6);
            if (!string.IsNullOrEmpty(validateAgentSenderId))
                recordErrors.AppendLine().AppendFormat(validateAgentSenderId);

            var oprator = record.Substring(20, 6);

            UInt16 year = 0;
            var strYear = record.Substring(26, 4).Length < 4 ? string.Empty : record.Substring(26, 4);
            UInt16.TryParse(strYear, out year);
            _msd1.Year = year;

            var validateYear = string.IsNullOrEmpty(record.Substring(26, 4).Trim()) || record.Substring(26, 4).Trim().Length < 4 || _msd1.Year == 0
                ? record.Substring(26, 4) + " is not a valid year. Please correct it."
                : string.Empty;
            if (!string.IsNullOrEmpty(validateYear))
                recordErrors.AppendLine().AppendFormat(validateYear);

            UInt16 qtr = 0;
            var strQtr = record.Substring(30, 1).Length < 1 ? string.Empty : record.Substring(30, 1);
            UInt16.TryParse(strQtr, out qtr);
            _msd1.Quarter = qtr;

            var validateQuarter = string.IsNullOrEmpty(record.Substring(30, 1).Trim()) || record.Substring(30, 1).Length < 1 || _msd1.Quarter == 0
                ? record.Substring(30, 1) + " is not a valid quarter. Please correct it."
                : string.Empty;
            if (!string.IsNullOrEmpty(validateQuarter))
                recordErrors.AppendLine().AppendFormat(validateQuarter);

            var validateReportingPort = !_helperService.IsValidReportingPort(record.Substring(31, 5)) || record.Substring(31, 5).Length < 5
                ? record.Substring(31, 5) + " is not a recognised code for reporting port. Enter a valid code or contact the helpdesk for advice."
                : string.Empty;

            _msd1.ReportingPort = !string.IsNullOrEmpty(validateReportingPort) ? string.Empty : record.Substring(31, 5);
            if (!string.IsNullOrEmpty(validateReportingPort))
                recordErrors.AppendLine().AppendFormat(validateReportingPort);

            bool validShipOrImoOrCallSign = false;
            string errorMessageIMO =
                " is not a recognised IMO number. Enter a valid IMO number or a Call Sign or Flag of Vessel and Vessel Name or contact the helpdesk for further advice";
            var validateImo = string.IsNullOrEmpty(record.Substring(36, 7).Trim()) || record.Substring(36, 7).Trim().Length < 7
                ? record.Substring(36, 7) + errorMessageIMO
                : String.Empty;

            UInt32 imoNum = 0;
            if (string.IsNullOrEmpty(validateImo))
            {
                var strImo = record.Substring(36, 7).Length < 7 ? string.Empty : record.Substring(36, 7);
                UInt32.TryParse(strImo, out imoNum);
                if (imoNum == 0)
                    validateImo = record.Substring(36, 7) + errorMessageIMO;
            }

            var callSign = string.IsNullOrEmpty(record.Substring(43, 9).Trim()) || record.Substring(43, 9).Length < 9
                ? string.Empty : record.Substring(43, 9);

            var flagOfVessel = string.IsNullOrEmpty(record.Substring(52, 3).Trim()) || record.Substring(52, 3).Length < 3
                ? string.Empty : record.Substring(52, 3);
            if (!flagOfVessel.Trim().All(Char.IsLetter))
            {
                flagOfVessel = string.Empty;
            }

            var shipName = string.Empty;
            if (record.Length >= 137)
            {
                shipName = string.IsNullOrEmpty(record.Substring(102, 35).Trim()) || record.Substring(102, 35).Length < 35 ? string.Empty : record.Substring(102, 35);
            }
            validShipOrImoOrCallSign = string.IsNullOrEmpty(validateImo) || !string.IsNullOrEmpty(callSign) || (!string.IsNullOrEmpty(flagOfVessel) && !string.IsNullOrEmpty(shipName));

            if (!validShipOrImoOrCallSign)
            {
                recordErrors.AppendLine().AppendFormat(validateImo);
            }

            _msd1.Imo = imoNum;
            _msd1.ShipName = shipName.Trim();
            _msd1.FlagCode = flagOfVessel.Trim();
            _msd1.Callsign = callSign.Trim();

            _msd1.IsInbound = record.Substring(55, 1) == "1";

            var validateVoyageDirection = record.Substring(55, 1).Equals("1") || record.Substring(55, 1).Equals("2") ? string.Empty :
                 record.Substring(55, 1) + " is not a valid value for Voyage Direction. Correct it to either 1 for inwards or 2 for outwards.";

            if (!string.IsNullOrEmpty(validateVoyageDirection))
                recordErrors.AppendLine().AppendFormat(validateVoyageDirection);

            var validateNumVoyages = string.IsNullOrEmpty(record.Substring(56, 6).Trim()) || record.Substring(56, 6).Length < 6
                ? record.Substring(56, 6) + " is not a valid number of voyages. Enter a number under 1000 or contact the helpdesk for advice."
                : string.Empty;

            if (!string.IsNullOrEmpty(validateNumVoyages))
                recordErrors.AppendLine().AppendFormat(validateNumVoyages);

            UInt32 numVoyages = 0;
            var strNumVoyages = record.Substring(56, 6).Length < 6 ? string.Empty : record.Substring(56, 6);
            UInt32.TryParse(strNumVoyages, out numVoyages);
            _msd1.NumVoyages = numVoyages;

            var validateAssociatedPort = !_helperService.IsValidPort(record.Substring(62, 5)) || record.Substring(62, 5).Length < 5
                ? record.Substring(62, 5) + " is not a recognised code for port of load/discharge. Enter a valid code or contact the helpdesk for advice."
                : string.Empty;

            _msd1.AssociatedPort = !string.IsNullOrEmpty(validateAssociatedPort) ? string.Empty : record.Substring(62, 5);
            if (!string.IsNullOrEmpty(validateAssociatedPort))
                recordErrors.AppendLine().AppendFormat(validateAssociatedPort);

            var validatePorts = !_msd1.ReportingPort.Equals(_msd1.AssociatedPort)
                ? string.Empty : "Port of load/discharge is the same as the reporting port. Please correct or contact the helpdesk for advice.";
            if (!string.IsNullOrEmpty(validatePorts))
                recordErrors.AppendLine().AppendFormat(validatePorts);

            _msd1.CreatedDate = DateTime.Now;
            _msd1.ModifiedDate = DateTime.Now;

            return string.IsNullOrEmpty(recordErrors.ToString().Trim()) ? true : false;
        }

        private void AddCargoSummaryToMsd1Data(string record)
        {
            _msd1CargoSummary = new Msd1CargoSummary
            {
                Msd1Id = _msd1.Msd1Id
            };

            var validateCategoryCode = string.IsNullOrEmpty(record.Substring(67, 2).Trim()) || record.Substring(67, 2).Length < 2
                ? record.Substring(67, 2) + " is not a valid cargo code. Please correct or contact the helpdesk for advice."
                : string.Empty;

            if (!string.IsNullOrEmpty(validateCategoryCode))
                recordErrors.AppendLine().AppendFormat(validateCategoryCode);

            byte categoryCode = 0;
            var strCategoryCode = record.Substring(67, 2).Length < 2 ? string.Empty : record.Substring(67, 2);
            byte.TryParse(strCategoryCode, out categoryCode);
            _msd1CargoSummary.CategoryCode = categoryCode;

            var validateGrossWeight = string.IsNullOrEmpty(record.Substring(69, 9).Trim())
                ? record.Substring(69, 9) + " is not a valid value for gross weight. Please correct or contact the helpdesk for advice."
                : string.Empty;

            if (!string.IsNullOrEmpty(validateGrossWeight))
                recordErrors.AppendLine().AppendFormat(validateGrossWeight);

            UInt32 grossWeight = 0;
            var strGrossWeight = record.Substring(69, 9).Length < 9 ? string.Empty : record.Substring(69, 9);
            UInt32.TryParse(strGrossWeight, out grossWeight);
            _msd1CargoSummary.GrossWeight = grossWeight;

            UInt32 unitsWithCargo = 0;
            var strUnitsWithCargo = record.Substring(78, 8).Length < 8 ? string.Empty : record.Substring(78, 8);
            UInt32.TryParse(strUnitsWithCargo, out unitsWithCargo);
            _msd1CargoSummary.UnitsWithCargo = unitsWithCargo;

            UInt32 unitsWithoutCargo = 0;
            var strUnitsWithoutCargo = record.Substring(86, 8).Length < 8 ? string.Empty : record.Substring(86, 8);
            UInt32.TryParse(strUnitsWithoutCargo, out unitsWithoutCargo);
            _msd1CargoSummary.UnitsWithoutCargo = unitsWithoutCargo;

            UInt32 totalUnits = 0;
            var strTotalUnits = record.Substring(94, 8).Length < 8 ? string.Empty : record.Substring(94, 8);
            UInt32.TryParse(strTotalUnits, out totalUnits);
            _msd1CargoSummary.TotalUnits = totalUnits;

            if (record.Length >= 137)
            {
                _msd1CargoSummary.Description = string.IsNullOrEmpty(record.Substring(137)) ? string.Empty : record.Substring(137);
            }

            ValidateMsd1CargoSummary(_msd1CargoSummary);
            if (string.IsNullOrEmpty(recordErrors.ToString().Trim()))
            {
                _msd1.Msd1CargoSummary.Add(_msd1CargoSummary);
            }
        }

        private void ValidateMsd1CargoSummary(Msd1CargoSummary msd1CargoSummary)
        {
            var (isValidMsd1, msd1HardValidationErrMsg) = _validateMsdData.ValidateMSd1CargoSummary(_msd1, msd1CargoSummary);
            if (!isValidMsd1 || !string.IsNullOrEmpty(msd1HardValidationErrMsg.Trim()))
            {
                recordErrors.Append(msd1HardValidationErrMsg);
            }
        }

        private void AddMsd1DataToDatabase()
        {
            if (!string.IsNullOrEmpty(_fileInfo.FileName))
            {
                _msd1.FileRefId = _fileInfo.FileRefId;
                _msd1DataService.Add(_msd1);
            }
            _msd1.Msd1CargoSummary.Clear();
        }
    }
}