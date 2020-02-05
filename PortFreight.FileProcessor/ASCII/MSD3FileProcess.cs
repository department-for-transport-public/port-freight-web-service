using Microsoft.Extensions.Logging;
using PortFreight.Data.Entities;
using PortFreight.FileProcess.ASCII;
using PortFreight.FileProcess.Common;
using PortFreight.Services.Common;
using PortFreight.Services.FileProcess;
using PortFreight.Services.MSD2;
using System;
using System.IO;
using System.Text;

namespace PortFreight.FileProcessor.ASCII
{
    public class MSD3FileProcess
    {
        private readonly ILogger _logger;
        private readonly IFileProcessService _fileProcessService;
        private readonly IMsd3DataService _msd3DataService;
        private IHelperService _helperService;
        private Msd3 _msd3;
        private Msd3agents _msd3Agent;
        private FlatFile _fileInfo;
        private StringBuilder bodyErrorMsg;
        private StringBuilder recordErrors;
        private ValidateMsdData _validateMsdData;

        public MSD3FileProcess(Msd3 msd3,
                              Msd3agents msd3Agent,
                              FlatFile fileInfo,
                              ValidateMsdData validateMsdData,
                              IHelperService helperService,
                              IFileProcessService fileProcessService,
                              IMsd3DataService msd3DataService,
                              ILogger<ASCIIFileProcess> logger
                            )
        {
            _msd3 = msd3;
            _msd3Agent = msd3Agent;
            _fileInfo = fileInfo;
            _validateMsdData = validateMsdData;
            _helperService = helperService;
            _fileProcessService = fileProcessService;
            _msd3DataService = msd3DataService;
            _logger = logger;
            recordErrors = new StringBuilder();
            bodyErrorMsg = new StringBuilder();
        }

        public string ProcessMsd3Records(string filename, FlatFile fileInfo)
        {
            _fileInfo = fileInfo;
            string record = string.Empty;
            bodyErrorMsg.Clear();

            var lines = File.ReadAllLines(filename);
            var noMsd3Data = lines.Length < 2 ? "There are no records to process in this file. Please correct it or contact the helpdesk for advice." : string.Empty;
            recordErrors.AppendLine(noMsd3Data).AppendLine();

            for (int i = 1; i < lines.Length; i++)
            {
                record = lines[i];
                int recordMinLength = 10;
                int startIndex = 8;
                try
                {
                    if (!string.IsNullOrEmpty(lines[i].Trim()))
                    {
                        int endIndex = Math.Min(record.Length - startIndex, recordMinLength);
                        int endIndexCurrent = record.Length - startIndex;
                        string currentLineLength = endIndexCurrent > startIndex && record.Length >= recordMinLength ? record.Substring(startIndex, endIndex) : string.Empty;

                        if (string.IsNullOrEmpty(currentLineLength))
                        {
                            recordErrors.AppendLine().AppendFormat('"' + record + '"' + " is not a valid record because it is missing some data");
                        }
                        else
                        {
                            int endIndexPrevious = lines[i - 1].Length - startIndex;
                            string previousLineLength = endIndexPrevious > startIndex && lines[i - 1].Length >= recordMinLength ? lines[i - 1].Substring(startIndex, Math.Min(endIndexPrevious, endIndex)) : string.Empty;
                            if (i == 1 || string.IsNullOrEmpty(lines[i - 1].Trim()) || !currentLineLength.Equals(previousLineLength))
                            {
                                _msd3.Id = _helperService.GetUniqueKey();
                            }
                            ValidateAndPopulateMsd3DataFromFlatFile(record, filename);
                            if (string.IsNullOrEmpty(recordErrors.ToString().Trim()))
                            {
                                AddShipplingLineAndAgents(record);
                                int endIndexNext = i == (lines.Length - 1) ? 0 : lines[i + 1].Length;
                                string nextLineLength = endIndexNext > startIndex && lines[i + 1].Length >= recordMinLength ? lines[i + 1].Substring(startIndex, endIndex) : string.Empty;
                                if (i == (lines.Length - 1) || !record.Substring(startIndex, endIndex).Equals(nextLineLength))
                                {
                                    AddMsd3DataToDatabase();
                                    _msd3.Msd3agents.Clear();
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
                    _logger.LogError("MSD3 Ascii Record error:", err.Message, err.InnerException != null ? err.InnerException.Message : string.Empty, err.StackTrace);

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

        private void AddShipplingLineAndAgents(string record)
        {
            _msd3Agent = new Msd3agents();
            var shippingLineOrAgent = record.Length < 24 || string.IsNullOrEmpty(record.Substring(18, 6).Trim()) ? string.Empty : record.Substring(18, 6);

            var validateShippingLineOrAgent = (record.Length > 18 && record.Length < 24) || string.IsNullOrEmpty(record.Substring(18, 6).Trim()) ?
                                       record.Substring(18, record.Length - 18) + " is not a recognised shipping line or agent. " +
                                       " Enter a valid shipping line/agent or contact the helpdesk for advice."
                                       : String.Empty;
            if (!string.IsNullOrEmpty(validateShippingLineOrAgent))
                recordErrors.AppendLine().AppendFormat(validateShippingLineOrAgent);

            if (!string.IsNullOrEmpty(shippingLineOrAgent))
            {
                _msd3Agent.Msd3Id = _msd3.Id;
                _msd3Agent.SenderId = shippingLineOrAgent.Trim();
            }

            if (string.IsNullOrEmpty(recordErrors.ToString().Trim()))
            {
                _msd3.Msd3agents.Add(_msd3Agent);
            }
        }

        private bool ValidateAndPopulateMsd3DataFromFlatFile(string record, string filename)
        {
            _msd3.DataSourceId = (int)DataSource.ASCII;
            _msd3.SenderId = _fileInfo.SenderId;

            var validateReportingPort = !_helperService.IsValidMsd3Port(record.Substring(8, 5))
                                        || record.Substring(8, 5).Length < 5 ?
                                        record.Substring(8, 5) + " is not a recognised code for reporting port. " +
                                        " Enter a valid code or contact the helpdesk for advice."
                                        : String.Empty;

            _msd3.ReportingPort = !string.IsNullOrEmpty(validateReportingPort) ? string.Empty : record.Substring(8, 5);
            if (!string.IsNullOrEmpty(validateReportingPort))
                recordErrors.AppendLine().AppendFormat(validateReportingPort);

            UInt16 year = 0;
            var strYear = record.Substring(13, 4).Length < 4 ? string.Empty : record.Substring(13, 4);
            UInt16.TryParse(strYear, out year);
            _msd3.Year = year;

            var validateYear = string.IsNullOrEmpty(record.Substring(13, 4).Trim())
                               || record.Substring(13, 4).Trim().Length < 4
                               || _msd3.Year == 0 ?
                               record.Substring(13, 4) + " is not a valid year. Please correct it."
                               : String.Empty;
            if (!string.IsNullOrEmpty(validateYear))
                recordErrors.AppendLine().AppendFormat(validateYear);

            UInt16 qtr = 0;
            var strQtr = string.IsNullOrEmpty(record.Substring(17, 1).Trim()) ? string.Empty : record.Substring(17, 1);
            UInt16.TryParse(strQtr, out qtr);
            _msd3.Quarter = qtr;

            var validateQuarter = string.IsNullOrEmpty(record.Substring(17, 1).Trim()) || _msd3.Quarter == 0 ?
                                 record.Substring(17, 1) + " is not a valid quarter. Please correct it."
                                : String.Empty;

            if (!string.IsNullOrEmpty(validateQuarter))
                recordErrors.AppendLine().AppendFormat(validateQuarter);

            var validateYearAndQtrSubmission = _validateMsdData.ValidateYearAndQtrSubmission(_msd3.Year, _msd3.Quarter);
            if (!string.IsNullOrEmpty(validateYearAndQtrSubmission))
                recordErrors.AppendLine().AppendFormat(validateYearAndQtrSubmission);

            _msd3.CreatedDate = DateTime.Now;
            _msd3.ModifiedDate = DateTime.Now;
            _msd3.CreatedBy = _validateMsdData.GetUserName(_fileInfo);
            _msd3.LastUpdatedBy = _validateMsdData.GetUserName(_fileInfo);

            return string.IsNullOrEmpty(recordErrors.ToString().Trim());
        }

        private void AddMsd3DataToDatabase()
        {
            if (string.IsNullOrEmpty(_fileInfo.FileName) || _msd3 == null)
            {
                return;
            }
            _msd3.FileRefId = _fileInfo.FileRefId;
            _msd3DataService.Add(_msd3);
        }
    }
}
