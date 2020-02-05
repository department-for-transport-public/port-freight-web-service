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
    public class Msd3FileProcess
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

        public Msd3FileProcess(Msd3 msd3,
                                Msd3agents msd3Agent,
                                FlatFile fileInfo,
                                ValidateMsdData validateMsdData,
                                IHelperService helperService,
                                IFileProcessService fileProcessService,
                                IMsd3DataService msd3DataService,
                                ILogger<Msd2FileProcess> logger
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

        public string ProcessMsd3Records(GesmesHelpers gesmesHelpers, string filename, FlatFile fileInfo)
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
                        string[] msd3Elements = gesmesHelpers.Split(":", lines[i].Replace("'", ""));

                        currentRecord = string.Join('|', msd3Elements, 0, 7);

                        string lastRecord = (i == 0) ? string.Empty : string.Join('|', gesmesHelpers.Split(":", lines[i - 1]), 0, 7);
                        string nextRecord = (i == (lines.Count - 1)) ? string.Empty : string.Join('|', gesmesHelpers.Split(":", lines[i + 1]), 0, 7);

                        if (i == 0 || !currentRecord.Equals(lastRecord))
                        {
                            _msd3.Id = _helperService.GetUniqueKey();
                            ValidateAndPopulateMsd3DataFromFlatFile(msd3Elements);
                        }

                        if ((string.IsNullOrEmpty(recordErrors.ToString().Trim())))
                        {
                            AddShipplingLineAndAgents(msd3Elements);
                            if (!(currentRecord.Equals(nextRecord)))
                            {
                                AddMsd3DataToDatabase();
                                _msd3.Msd3agents.Clear();
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

        private void AddShipplingLineAndAgents(string[] msd3Elements)
        {
            _msd3Agent = new Msd3agents();
            _msd3Agent.Msd3Id = _msd3.Id;

            _msd3Agent = new Msd3agents();


            var validateShippingLineOrAgent = string.IsNullOrEmpty(msd3Elements[7]) ?
                                          msd3Elements[7] + " is not a recognised code for reporting port. " +
                                         " Enter a valid code or contact the helpdesk for advice."
                                         : String.Empty;

            var shippingLineOrAgent = !string.IsNullOrEmpty(validateShippingLineOrAgent) ? string.Empty : msd3Elements[7];

            if (!string.IsNullOrEmpty(validateShippingLineOrAgent))
                recordErrors.AppendLine().AppendFormat(validateShippingLineOrAgent);

            if (!string.IsNullOrEmpty(shippingLineOrAgent))
            {
                _msd3Agent.Msd3Id = _msd3.Id;
                _msd3Agent.SenderId = shippingLineOrAgent.Replace("'", "");
            }

            if (string.IsNullOrEmpty(recordErrors.ToString().Trim()))
            {
                _msd3.Msd3agents.Add(_msd3Agent);
            }
        }

        private void ValidateAndPopulateMsd3DataFromFlatFile(string[] msd3Elements)
        {
            _msd3.LastUpdatedBy = _validateMsdData.GetUserName(_fileInfo);
            _msd3.CreatedBy = _validateMsdData.GetUserName(_fileInfo);
            _msd3.DataSourceId = (int)DataSource.GESMES;
            _msd3.CreatedDate = DateTime.Now;
            _msd3.ModifiedDate = DateTime.Now;
            _msd3.SenderId = _fileInfo.SenderId;

            msd3Elements[0] = msd3Elements[0].Replace("ARR", "").Replace("++", "").Replace(" ", "");

            var validateReportingPort = !_helperService.IsValidMsd3Port(msd3Elements[0])
                                         || string.IsNullOrEmpty(msd3Elements[0]) ?
                                          msd3Elements[0] + " is not a recognised code for reporting port. " +
                                         " Enter a valid code or contact the helpdesk for advice."
                                         : String.Empty;

            _msd3.ReportingPort = !string.IsNullOrEmpty(validateReportingPort) ? string.Empty : msd3Elements[0];
            if (!string.IsNullOrEmpty(validateReportingPort))
                recordErrors.AppendLine().AppendFormat(validateReportingPort);

            UInt16 year = 0;
            var strYear = string.IsNullOrEmpty(msd3Elements[1].Trim()) || msd3Elements[1].Trim().Length < 4 ? string.Empty : msd3Elements[1];
            UInt16.TryParse(strYear, out year);
            _msd3.Year = year;

            var validateYear = _msd3.Year.ToString().Length < 4 || _msd3.Year == 0 ?
                msd3Elements[1].Trim() + " is not a valid year. Please correct it."
                : String.Empty;
            if (!string.IsNullOrEmpty(validateYear))
                recordErrors.AppendLine().AppendFormat(validateYear);

            UInt16 qtr = 0;
            var strQtr = string.IsNullOrEmpty(msd3Elements[2].Trim()) || msd3Elements[2].Trim().Length < 1 ? string.Empty : msd3Elements[2];
            UInt16.TryParse(strQtr, out qtr);
            _msd3.Quarter = qtr;

            var validateQuarter = _msd3.Quarter == 0 ?
                 msd3Elements[2].Trim() + " is not a valid quarter. Please correct it."
                : String.Empty;

            if (!string.IsNullOrEmpty(validateQuarter))
                recordErrors.AppendLine().AppendFormat(validateQuarter);

            var validateYearAndQtrSubmission = _validateMsdData.ValidateYearAndQtrSubmission(_msd3.Year, _msd3.Quarter);
            if (!string.IsNullOrEmpty(validateYearAndQtrSubmission))
                recordErrors.AppendLine().AppendFormat(validateYearAndQtrSubmission);

            _msd3.FileRefId = null;
        }
        private void AddMsd3DataToDatabase()
        {
            if (string.IsNullOrEmpty(_fileInfo.FileName) || _msd3 == null) return;
            _msd3.FileRefId = _fileInfo.FileRefId;
            _msd3DataService.Add(_msd3);
        }

    }
}
