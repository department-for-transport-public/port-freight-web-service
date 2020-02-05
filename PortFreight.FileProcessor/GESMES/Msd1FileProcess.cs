using Microsoft.Extensions.Logging;
using PortFreight.Data.Entities;
using PortFreight.FileProcess.Common;
using PortFreight.FileProcess.GESMES;
using PortFreight.Services.Common;
using PortFreight.Services.FileProcess;
using PortFreight.Services.MSD1;
using System;
using System.IO;
using System.Linq;
using System.Text;


namespace PortFreight.FileProcessor.GESMES
{
    public class Msd1FileProcess
    {
        private Msd1Data _msd1;
        private FlatFile _fileInfo;
        private readonly ILogger _logger;
        private readonly IHelperService _helperService;
        private readonly IFileProcessService _fileProcessService;
        private readonly IMsd1DataService _msd1DataService;
        private GesmesHelpers _gesmesHelpers;
        private ValidateMsdData _validateMsdData;
        private StringBuilder recordErrors;
        private StringBuilder bodyErrorMsg;

        public Msd1FileProcess(Msd1Data msd1,
            FlatFile fileInfo,
            GesmesHelpers gesmesHelpers,
            ValidateMsdData validateMsdData,
            IMsd1DataService msd1DataService,
            IFileProcessService fileProcessService,
            IHelperService helperService,
            ILogger<Msd1FileProcess> logger)
        {
            _msd1 = msd1;
            _fileInfo = fileInfo;
            _gesmesHelpers = gesmesHelpers;
            _helperService = helperService;
            _msd1DataService = msd1DataService;
            _validateMsdData = validateMsdData;
            _fileProcessService = fileProcessService;
            _logger = logger;
            recordErrors = new StringBuilder();
            bodyErrorMsg = new StringBuilder();
        }

        public string ProcessMsd1Records(GesmesHelpers gesmesHelpers, string filename, FlatFile fileInfo)
        {
            _fileInfo = fileInfo;
            string record = string.Empty;
            bodyErrorMsg.Clear();

            var allLines = File.ReadAllLines(filename);
            var lines = allLines.Where(c => c.Contains("ARR")).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                string currentRecord = string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(lines[i]) && lines[i].Substring(0, 3) == "ARR")
                    {
                        string[] msd1Elements = gesmesHelpers.Split(":", lines[i].Replace("'", ""));
                        currentRecord = string.Join('|', msd1Elements, 0, 13);
                        string lastRecord = (i == 0) ? string.Empty : string.Join('|', gesmesHelpers.Split(":", lines[i - 1]), 0, 13);
                        string nextRecord = (i == (lines.Count - 1)) ? string.Empty : string.Join('|', gesmesHelpers.Split(":", lines[i + 1]), 0, 13);

                        if (i == 0 || !currentRecord.Equals(lastRecord))
                        {
                            _msd1.Msd1Id = _helperService.GetUniqueKey();
                        }
                        ValidateAndPopulateMsd1DataFromFlatFile(msd1Elements);
                        if (string.IsNullOrEmpty(recordErrors.ToString().Trim()))
                        {
                            AddCargoSummaryToMsd1Data(msd1Elements);
                            if (!currentRecord.Equals(nextRecord))
                            {
                                AddMsd1DataToDatabase();
                            }
                        }
                        if (!(string.IsNullOrEmpty(recordErrors.ToString().Trim())))
                        {
                            bodyErrorMsg.AppendLine(" #Line from your file").AppendLine().AppendLine(lines[i])
                                .AppendLine(" #What is wrong and how to fix it").AppendLine().Append(recordErrors)
                                .AppendLine().Append(" ___").AppendLine();
                            recordErrors.Clear();
                        }
                    }
                }
                catch (Exception err)
                {
                    _logger.LogError("Gesmes Record error:", err.Message, err.InnerException != null ? err.InnerException.Message : string.Empty, err.StackTrace);

                    if (_fileInfo == null) continue;
                    recordErrors.AppendLine(err.Message).Append(" Please correct it or contact the help-desk for advice.");
                    bodyErrorMsg.AppendLine(" #Line from your file").AppendLine().AppendLine(lines[i])
                        .AppendLine(" #What is wrong and how to fix it").AppendLine().Append(recordErrors)
                        .AppendLine().Append(" ___").AppendLine();
                    recordErrors.Clear();
                }
            }
            return bodyErrorMsg.ToString();
        }

        private void ValidateAndPopulateMsd1DataFromFlatFile(string[] msd1Elements)
        {
            _msd1.UserName = _validateMsdData.GetUserName(_fileInfo);
            _msd1.DataSourceId = (int)DataSource.GESMES;

            _msd1.RecordRef = "";

            var validateLinSenderId = msd1Elements[0].Length < 6 ?
                msd1Elements[0].Trim() + " is not a recognised code for shipping line. Replace it or contact the help-desk for advice." : String.Empty;

            _msd1.LineSenderId = !string.IsNullOrEmpty(validateLinSenderId) ? string.Empty : msd1Elements[0].ToString().Replace("ARR++", "");
            if (!string.IsNullOrEmpty(validateLinSenderId))
                recordErrors.AppendLine().AppendFormat(validateLinSenderId);

            var validateAgentSenderId = msd1Elements[1].Length < 6 ?
                msd1Elements[1].Trim() + " is not a recognised code for shipping agent. Replace it or contact the help-desk for advice." : String.Empty;

            _msd1.AgentSenderId = !string.IsNullOrEmpty(validateAgentSenderId) ? string.Empty : msd1Elements[1];
            if (!string.IsNullOrEmpty(validateAgentSenderId))
                recordErrors.AppendLine().AppendFormat(validateAgentSenderId);

            UInt16 year = 0;
            var strYear = string.IsNullOrEmpty(msd1Elements[3].Trim()) || msd1Elements[3].Trim().Length < 4 ? string.Empty : msd1Elements[3];
            UInt16.TryParse(strYear, out year);
            _msd1.Year = year;

            var validateYear = _msd1.Year.ToString().Length < 4 || _msd1.Year == 0 ? msd1Elements[3].Trim() + " is not a valid year. Please correct it." : String.Empty;
            if (!string.IsNullOrEmpty(validateYear))
                recordErrors.AppendLine().AppendFormat(validateYear);

            UInt16 qtr = 0;
            var strQtr = string.IsNullOrEmpty(msd1Elements[4].Trim()) || msd1Elements[4].Trim().Length < 1 ? string.Empty : msd1Elements[4];
            UInt16.TryParse(strQtr, out qtr);
            _msd1.Quarter = qtr;

            var validateQuarter = _msd1.Quarter == 0 ? msd1Elements[4].Trim() + " is not a valid quarter. Please correct it." : String.Empty;

            if (!string.IsNullOrEmpty(validateQuarter))
                recordErrors.AppendLine().AppendFormat(validateQuarter);

            _msd1.ReportingPort = !_helperService.IsValidReportingPort(msd1Elements[5].Trim()) || string.IsNullOrEmpty(msd1Elements[5].Trim()) ? string.Empty : msd1Elements[5];
            var validateReportingPort = string.IsNullOrEmpty(_msd1.ReportingPort.Trim()) ?
                msd1Elements[5].Trim() + " is not a recognised code for reporting port. Enter a valid code or contact the help-desk for advice." : String.Empty;

            if (!string.IsNullOrEmpty(validateReportingPort))
                recordErrors.AppendLine().AppendFormat(validateReportingPort);

            bool validShipOrImoOrCallSign = false;
            string errorMessageIMO = " is not a recognised IMO number. Enter a valid IMO number or a Call Sign or Flag of Vessel and Vessel Name or contact the helpdesk for further advice";
            var validateImo = string.IsNullOrEmpty(msd1Elements[6].Trim()) || msd1Elements[6].Trim().Length < 7 || msd1Elements[6].Trim().Length > 7 ? msd1Elements[6] + errorMessageIMO : String.Empty;

            UInt32 imoNum = 0;
            if (string.IsNullOrEmpty(validateImo))
            {
                var strImo = msd1Elements[6].Length < 7 ? string.Empty : msd1Elements[6];
                UInt32.TryParse(strImo, out imoNum);
                if (imoNum == 0) { validateImo = msd1Elements[6] + errorMessageIMO; }
            }

            var callSign = string.IsNullOrEmpty(msd1Elements[7].Trim()) || msd1Elements[7].Length > 9 ? string.Empty : msd1Elements[7];
            var flagOfVessel = string.IsNullOrEmpty(msd1Elements[9].Trim()) || msd1Elements[9].Length != 3 ? string.Empty : msd1Elements[9];
            if (!flagOfVessel.Trim().All(Char.IsLetter)) { flagOfVessel = string.Empty; }
            var shipName = string.IsNullOrEmpty(msd1Elements[8].Trim()) ? string.Empty : msd1Elements[8];

            validShipOrImoOrCallSign = string.IsNullOrEmpty(validateImo) || !string.IsNullOrEmpty(callSign) || (!string.IsNullOrEmpty(flagOfVessel) && !string.IsNullOrEmpty(shipName));
            if (!validShipOrImoOrCallSign)
            {
                recordErrors.AppendLine().AppendFormat(validateImo);
            }

            _msd1.Imo = imoNum;
            _msd1.ShipName = shipName.Substring(0, shipName.Length >= 35 ? 35 : shipName.Length).Trim();
            _msd1.FlagCode = flagOfVessel.Trim();
            _msd1.Callsign = callSign.Trim();

            _msd1.IsInbound = msd1Elements[10] == "1";
            var validateVoyageDirection = msd1Elements[10] == "1" || msd1Elements[10] == "2" ?
                string.Empty : msd1Elements[10].Trim() + " is not a valid value for Voyage Direction. Correct it to either 1 for inwards or 2 for outwards.";
            if (!string.IsNullOrEmpty(validateVoyageDirection))
                recordErrors.AppendLine().AppendFormat(validateVoyageDirection);

            _msd1.NumVoyages = string.IsNullOrEmpty(msd1Elements[11].Trim()) ? Convert.ToUInt32(0) : Convert.ToUInt32(msd1Elements[11]);

            UInt32 numVoyages = 0;
            var strNumVoyages = string.IsNullOrEmpty(msd1Elements[11].Trim()) ? string.Empty : msd1Elements[11];
            UInt32.TryParse(strNumVoyages, out numVoyages);
            _msd1.NumVoyages = numVoyages;

            var validateNumVoyages = string.IsNullOrEmpty(msd1Elements[11].Trim()) ?
                msd1Elements[11].Trim() + " is not a valid number of voyages. Enter a number under 1000 or contact the help-desk for further advice." : String.Empty;
            if (!string.IsNullOrEmpty(validateNumVoyages))
                recordErrors.AppendLine().AppendFormat(validateNumVoyages);

            _msd1.AssociatedPort = !_helperService.IsValidPort(msd1Elements[12].Trim()) || string.IsNullOrEmpty(msd1Elements[12].Trim()) ? string.Empty : msd1Elements[12];
            var validateAssociatedPort = string.IsNullOrEmpty(_msd1.AssociatedPort.Trim()) ?
                msd1Elements[12].Trim() + " is not a recognised code for port of load/discharge. Enter a valid code or contact the help-desk for further advice." : String.Empty;

            if (!string.IsNullOrEmpty(validateAssociatedPort))
                recordErrors.AppendLine().AppendFormat(validateAssociatedPort);

            var ValidatePorts = !_msd1.ReportingPort.Equals(_msd1.AssociatedPort) ?
                string.Empty : "Port of load/discharge is the same as the reporting port. Please correct or contact the help-desk for further advice.";
            if (!string.IsNullOrEmpty(ValidatePorts))
            {
                recordErrors.AppendLine().AppendFormat(ValidatePorts);
            }

            _msd1.CreatedDate = DateTime.Now;
            _msd1.ModifiedDate = DateTime.Now;
            _msd1.FileRefId = null;
        }

        private void AddCargoSummaryToMsd1Data(string[] ARR)
        {
            var msd1CargoCategory = new Msd1CargoSummary();
            msd1CargoCategory.Msd1Id = _msd1.Msd1Id;

            Byte categoryCode = 0;
            var strCategoryCode = string.IsNullOrEmpty(ARR[13].Trim()) ? string.Empty : ARR[13];
            Byte.TryParse(strCategoryCode, out categoryCode);
            msd1CargoCategory.CategoryCode = categoryCode;

            var validateCategoryCode = string.IsNullOrEmpty(ARR[13].Trim()) ?
                ARR[13].Trim() + " is not a valid cargo code." + " Please correct or contact the help-desk for advice." : String.Empty;
            if (!string.IsNullOrEmpty(validateCategoryCode))
            {
                recordErrors.AppendLine().AppendFormat(validateCategoryCode);
            }

            UInt32 grossWeight = 0;
            var strGrossWeight = string.IsNullOrEmpty(ARR[14].Trim()) ? string.Empty : ARR[14];
            UInt32.TryParse(strGrossWeight, out grossWeight);
            msd1CargoCategory.GrossWeight = grossWeight;

            var validateGrossWeight = string.IsNullOrEmpty(ARR[14].Trim()) ?
                  ARR[14].Trim() + " is not a valid value for gross weight. Please correct or contact the help-desk for advice." : String.Empty;
            if (!string.IsNullOrEmpty(validateGrossWeight))
            {
                recordErrors.AppendLine().AppendFormat(validateGrossWeight);
            }

            UInt32 unitsWithCargo = 0;
            var strUnitsWithCargo = string.IsNullOrEmpty(ARR[15].Trim()) ? string.Empty : ARR[15];
            UInt32.TryParse(strUnitsWithCargo, out unitsWithCargo);
            msd1CargoCategory.UnitsWithCargo = unitsWithCargo;

            var validateUnitsWithCargo = string.IsNullOrEmpty(ARR[15].Trim()) ?
                ARR[15].Trim() + " is not a valid value for units with cargo. Please correct or contact the help-desk for advice." : String.Empty;
            if (!string.IsNullOrEmpty(validateUnitsWithCargo))
            {
                recordErrors.AppendLine().AppendFormat(validateUnitsWithCargo);
            }

            UInt32 unitsWithoutCargo = 0;
            var strUnitsWithoutCargo = string.IsNullOrEmpty(ARR[16].Trim()) ? string.Empty : ARR[16];
            UInt32.TryParse(strUnitsWithoutCargo, out unitsWithoutCargo);
            msd1CargoCategory.UnitsWithoutCargo = unitsWithoutCargo;

            var validateUnitsWithoutCargo = string.IsNullOrEmpty(ARR[16].Trim()) ?
                 ARR[16].Trim() + " is not a valid value for units without cargo. Please correct or contact the help-desk for advice." : String.Empty;
            if (!string.IsNullOrEmpty(validateUnitsWithoutCargo))
            {
                recordErrors.AppendLine().AppendFormat(validateUnitsWithoutCargo);
            }

            UInt32 totalUnits = 0;
            var strTotalUnits = string.IsNullOrEmpty(ARR[17].Trim()) ? string.Empty : ARR[17];
            UInt32.TryParse(strTotalUnits, out totalUnits);
            msd1CargoCategory.TotalUnits = totalUnits;

            var validateTotalUnits = string.IsNullOrEmpty(ARR[17].Trim()) ?
                ARR[17].Trim() + " is not a valid value for total units. Please correct or contact the help-desk for advice." : String.Empty;
            if (!string.IsNullOrEmpty(validateTotalUnits))
            {
                recordErrors.AppendLine().AppendFormat(validateTotalUnits);
            }

            ValidateMsd1CargoSummary(msd1CargoCategory);

            if (string.IsNullOrEmpty(recordErrors.ToString().Trim()))
            {
                _msd1.Msd1CargoSummary.Add(msd1CargoCategory);
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
            if (!string.IsNullOrEmpty(_fileInfo.FileName) && string.IsNullOrEmpty(recordErrors.ToString().Trim()))
            {
                _msd1.FileRefId = _fileInfo.FileRefId;
                _msd1DataService.Add(_msd1);
            }
            _msd1.Msd1CargoSummary.Clear();
        }
    }
}
