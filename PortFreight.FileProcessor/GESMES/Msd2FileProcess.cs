using Microsoft.Extensions.Logging;
using PortFreight.Data.Entities;
using PortFreight.FileProcess.Common;
using PortFreight.FileProcess.GESMES;
using PortFreight.Services.Common;
using PortFreight.Services.FileProcess;
using PortFreight.Services.MSD2;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PortFreight.FileProcessor.GESMES
{
    public class Msd2FileProcess
    {
        private readonly ILogger _logger;
        private readonly IFileProcessService _fileProcessService;
        private readonly IMsd2DataService _msd2DataService;
        private IHelperService _helperService;
        private Msd2 _msd2;
        private FlatFile _fileInfo;
        private StringBuilder bodyErrorMsg;
        private StringBuilder recordErrors;
        private ValidateMsdData _validateMsdData;

        public Msd2FileProcess(Msd2 msd2,
                             FlatFile fileInfo,
                             ValidateMsdData validateMsdData,
                             IHelperService helperService,
                             IFileProcessService fileProcessService,
                             IMsd2DataService msd2DataService,
                             ILogger<Msd2FileProcess> logger
                           )
        {
            _msd2 = msd2;
            _fileInfo = fileInfo;
            _validateMsdData = validateMsdData;
            _helperService = helperService;
            _fileProcessService = fileProcessService;
            _msd2DataService = msd2DataService;
            _logger = logger;
            recordErrors = new StringBuilder();
            bodyErrorMsg = new StringBuilder();
        }

        public string ProcessMsd2Records(GesmesHelpers gesmesHelpers, string filename, FlatFile fileInfo)
        {
            _fileInfo = fileInfo;
            string record = string.Empty;

            var allLines = File.ReadAllLines(filename);
            var lines = allLines.Where(c => c.Contains("ARR")).ToList();           

            for (int i = 0; i < lines.Count; i++)
            {
                string currentRecord = string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(lines[i]) && lines[i].Substring(0, 3) == "ARR")
                    {
                        string[] msd2Elements = gesmesHelpers.Split(":", lines[i].Replace("'", ""));

                        currentRecord = string.Join('|', msd2Elements, 0, 7);

                        string lastRecord = (i == 0) ? string.Empty : string.Join('|', gesmesHelpers.Split(":", lines[i - 1]), 0, 7);
                        string nextRecord = (i == (lines.Count - 1)) ? string.Empty : string.Join('|', gesmesHelpers.Split(":", lines[i + 1]), 0, 7);
                       
                        ValidateAndPopulateMsd2DataFromFlatFile(msd2Elements);
                        if ((string.IsNullOrEmpty(recordErrors.ToString().Trim())))
                        {
                            if (!(currentRecord.Equals(nextRecord)))
                            {
                                AddMsd2DataToDatabase();
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

                    if (_fileInfo != null)
                    {
                        recordErrors.AppendLine(err.Message).Append(" Please correct it or contact the helpdesk for advice.");
                        bodyErrorMsg.AppendLine(" #Line from your file").AppendLine().AppendLine(lines[i])
                                    .AppendLine(" #What is wrong and how to fix it").AppendLine().Append(recordErrors)
                                    .AppendLine().Append(" ___").AppendLine();
                        recordErrors.Clear();
                    }
                }
            }
            return bodyErrorMsg.ToString();            
        }

        private void ValidateAndPopulateMsd2DataFromFlatFile(string[] msd2Elements)
        {
            _msd2.LastUpdatedBy = _validateMsdData.GetUserName(_fileInfo);
            _msd2.CreatedBy = _validateMsdData.GetUserName(_fileInfo);
            _msd2.DataSourceId = (int)DataSource.GESMES;
            _msd2.CreatedDate = DateTime.Now;
            _msd2.ModifiedDate = DateTime.Now;
            _msd2.SenderId = _fileInfo.SenderId;

            msd2Elements[0] = msd2Elements[0].Replace("ARR", "").Replace("++", "");

            var validateReportingPort = !_helperService.IsValidMsd2Port(msd2Elements[0])
                                         || msd2Elements[0].Length < 5 ?
                                          msd2Elements[0] + " is not a recognised code for reporting port. " +
                                         " Enter a valid code or contact the helpdesk for advice."
                                         : String.Empty;

            _msd2.ReportingPort = !string.IsNullOrEmpty(validateReportingPort) ? string.Empty : msd2Elements[0];
            if (!string.IsNullOrEmpty(validateReportingPort))
                recordErrors.AppendLine().AppendFormat(validateReportingPort);

            UInt16 year = 0;
            var strYear = string.IsNullOrEmpty(msd2Elements[1].Trim()) || msd2Elements[1].Trim().Length < 4 ? string.Empty : msd2Elements[1];
            UInt16.TryParse(strYear, out year);
            _msd2.Year = year;

            var validateYear = _msd2.Year.ToString().Length < 4 || _msd2.Year == 0 ?
                msd2Elements[1].Trim() + " is not a valid year. Please correct it."
                : String.Empty;
            if (!string.IsNullOrEmpty(validateYear))
                recordErrors.AppendLine().AppendFormat(validateYear);

            UInt16 qtr = 0;
            var strQtr = string.IsNullOrEmpty(msd2Elements[2].Trim()) || msd2Elements[2].Trim().Length < 1 ? string.Empty : msd2Elements[2];
            UInt16.TryParse(strQtr, out qtr);
            _msd2.Quarter = qtr;

            var validateQuarter = _msd2.Quarter == 0 ?
                 msd2Elements[2].Trim() + " is not a valid quarter. Please correct it."
                : String.Empty;

            if (!string.IsNullOrEmpty(validateQuarter))
                recordErrors.AppendLine().AppendFormat(validateQuarter);

            decimal grossWeightInWard = 0.0m;
            var strGrossWeightInward = string.IsNullOrEmpty(msd2Elements[3]) ? string.Empty : msd2Elements[3];
            Decimal.TryParse(strGrossWeightInward, out grossWeightInWard);
            _msd2.GrossWeightInward = grossWeightInWard;

            decimal grossWeightOutWard = 0.0m;
            var strGrossWeightOutward = string.IsNullOrEmpty(msd2Elements[4]) ? string.Empty : msd2Elements[4];
            Decimal.TryParse(strGrossWeightOutward, out grossWeightOutWard);
            _msd2.GrossWeightOutward = grossWeightOutWard;


            UInt32 totalUnitsInwards = 0;
            var strTotalUnitsInwards = string.IsNullOrEmpty(msd2Elements[5]) ? string.Empty : msd2Elements[5];
            UInt32.TryParse(strTotalUnitsInwards, out totalUnitsInwards);
            _msd2.TotalUnitsInward = totalUnitsInwards;

            UInt32 totalUnitsOutwards = 0;
            var strTotalUnitsOutwards = string.IsNullOrEmpty(msd2Elements[6]) ? string.Empty : msd2Elements[6];
            UInt32.TryParse(strTotalUnitsOutwards, out totalUnitsOutwards);
            _msd2.TotalUnitsOutward = totalUnitsOutwards;
         
            _msd2.PassengerVehiclesInward = 0;
            _msd2.PassengerVehiclesOutward = 0;

            var validateGrossWeights = _validateMsdData.ValidateMSD2InwardOutwadCargoWeight(_msd2);
            if (!string.IsNullOrEmpty(validateGrossWeights))
                recordErrors.AppendLine().AppendFormat(validateGrossWeights);

            var validateYearAndQtrSubmission = _validateMsdData.ValidateYearAndQtrSubmission(_msd2.Year, _msd2.Quarter);
            if (!string.IsNullOrEmpty(validateYearAndQtrSubmission))
                recordErrors.AppendLine().AppendFormat(validateYearAndQtrSubmission);

            _msd2.FileRefId = null;
        }
        private void AddMsd2DataToDatabase()
        {
            if (!(string.IsNullOrEmpty(_fileInfo.FileName)) && _msd2 != null)
            {
                _msd2.FileRefId = _fileInfo.FileRefId;
                _msd2DataService.Add(_msd2);
            }
        }

    }
}
