using Microsoft.Extensions.Logging;
using PortFreight.Data.Entities;
using PortFreight.FileProcess.Common;
using PortFreight.Services.Common;
using PortFreight.Services.FileProcess;
using PortFreight.Services.MSD2;
using System;
using System.IO;
using System.Text;

namespace PortFreight.FileProcessor.ASCII
{
    public class MSD2FileProcess
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

        public MSD2FileProcess(Msd2 msd2,
                              FlatFile fileInfo,
                              ValidateMsdData validateMsdData,
                              IHelperService helperService,
                              IFileProcessService fileProcessService,
                              IMsd2DataService msd2DataService,
                              ILogger<MSD2FileProcess> logger
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

        public string ProcessMsd2Records(string filename, FlatFile fileInfo)
        {
            _fileInfo = fileInfo;
            string record = string.Empty;

            var lines = File.ReadAllLines(filename);
            var noMsd2Data = lines.Length < 2 ? "There are no records to process in this file. Please correct it or contact the helpdesk for advice." : string.Empty;
            recordErrors.AppendLine(noMsd2Data).AppendLine();

            for (int i = 1; i < lines.Length; i++)
            {
                record = lines[i];
                try
                {
                    if (!string.IsNullOrEmpty(lines[i].Trim()))
                    {
                        ValidateAndPopulateMsd2DataFromFlatFile(record, filename);
                        if (string.IsNullOrEmpty(recordErrors.ToString().Trim()) && _msd2.SenderId != null)
                        {
                            AddMsd2DataToDatabase();
                        }
                        else
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
                    _logger.LogError("MSD2 Ascii Record error:", err.Message, err.InnerException != null ? err.InnerException.Message : string.Empty, err.StackTrace);

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

        private bool ValidateAndPopulateMsd2DataFromFlatFile(string record, string filename)
        {
            if (record.Length < 52)
            {
                recordErrors.AppendLine().AppendFormat('"' + record + '"' + " is not a valid record because it is missing some data");
            }
            else
            {
                _msd2.DataSourceId = (int)DataSource.ASCII;
                _msd2.SenderId = _fileInfo.SenderId;
                var validateReportingPort = !_helperService.IsValidMsd2Port(record.Substring(8, 5))
                                            || record.Substring(8, 5).Length < 5 ?
                                            record.Substring(8, 5) + " is not a recognised code for reporting port. " +
                                            " Enter a valid code or contact the helpdesk for advice."
                                            : String.Empty;

                _msd2.ReportingPort = !string.IsNullOrEmpty(validateReportingPort) ? string.Empty : record.Substring(8, 5);
                if (!string.IsNullOrEmpty(validateReportingPort))
                    recordErrors.AppendLine().AppendFormat(validateReportingPort);

                UInt16 year = 0;
                var strYear = record.Substring(13, 4).Length < 4 ? string.Empty : record.Substring(13, 4);
                UInt16.TryParse(strYear, out year);
                _msd2.Year = year;

                var validateYear = string.IsNullOrEmpty(record.Substring(13, 4).Trim())
                                   || record.Substring(13, 4).Trim().Length < 4 || _msd2.Year == 0
                    ? record.Substring(13, 4) + " is not a valid year. Please correct it."
                    : String.Empty;

                if (!string.IsNullOrEmpty(validateYear))
                    recordErrors.AppendLine().AppendFormat(validateYear);

                UInt16 qtr = 0;
                var strQtr = record.Substring(17, 1).Length < 1 ? string.Empty : record.Substring(17, 1);
                UInt16.TryParse(strQtr, out qtr);
                _msd2.Quarter = qtr;

                var validateQuarter = string.IsNullOrEmpty(record.Substring(17, 1).Trim())
                                      || record.Substring(17, 1).Length < 1 || _msd2.Quarter == 0
                    ? record.Substring(30, 1) + " is not a valid quarter. Please correct it."
                    : String.Empty;

                if (!string.IsNullOrEmpty(validateQuarter))
                    recordErrors.AppendLine().AppendFormat(validateQuarter);

                decimal grossWeightInWard = 0.0m;
                var strGrossWeightInward = record.Substring(18, 9).Length < 9 ? string.Empty : record.Substring(18, 9);
                Decimal.TryParse(strGrossWeightInward, out grossWeightInWard);
                _msd2.GrossWeightInward = grossWeightInWard;

                decimal grossWeightOutWard = 0.0m;
                var strGrossWeightOutward = record.Substring(27, 9).Length < 9 ? string.Empty : record.Substring(27, 9);
                Decimal.TryParse(strGrossWeightOutward, out grossWeightOutWard);
                _msd2.GrossWeightOutward = grossWeightOutWard;

                UInt32 totalUnitsInwards = 0;
                var strTotalUnitsInwards = record.Substring(36, 8).Length < 8 ? string.Empty : record.Substring(36, 8);
                UInt32.TryParse(strTotalUnitsInwards, out totalUnitsInwards);
                _msd2.TotalUnitsInward = totalUnitsInwards;

                UInt32 totalUnitsOutwards = 0;
                var strTotalUnitsOutwards = record.Substring(44, 8).Length < 8 ? string.Empty : record.Substring(44, 8);
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

                _msd2.CreatedDate = DateTime.Now;
                _msd2.ModifiedDate = DateTime.Now;
                _msd2.CreatedBy = _validateMsdData.GetUserName(_fileInfo);
                _msd2.LastUpdatedBy = _validateMsdData.GetUserName(_fileInfo);
            }

            return string.IsNullOrEmpty(recordErrors.ToString().Trim()) ? true : false;
        }
        private void AddMsd2DataToDatabase()
        {
            if (string.IsNullOrEmpty(_fileInfo.FileName) || _msd2 == null)
            {
                return;
            }
            _msd2.FileRefId = _fileInfo.FileRefId;
            _msd2DataService.Add(_msd2);
        }
    }
}
