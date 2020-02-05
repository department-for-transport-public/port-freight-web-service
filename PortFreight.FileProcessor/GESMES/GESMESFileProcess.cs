using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortFreight.Data.Entities;
using PortFreight.FileProcess.Common;
using PortFreight.FileProcessor.GESMES;
using PortFreight.Services.Common;
using PortFreight.Services.EmailSender;
using PortFreight.Services.FileProcess;
using PortFreight.Services.MSD1;
using PortFreight.Services.MSD2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PortFreight.FileProcess.GESMES
{
    public class GESMESFileProcess
    {

        private readonly ILogger _logger;
        private readonly IHelperService _helperService;
        private readonly IFileProcessService _fileProcessService;
        private readonly IMsd1DataService _msd1DataService;
        private readonly IMsd2DataService _msd2DataService;
        private readonly IMsd3DataService _msd3DataService;
        private readonly IEmailSender _emailSender;
        private LogFileData _logFileData;
        private FlatFile _fileInfo;
        private GesmesHelpers _gesmesHelpers;
        private ValidateMsdData _validateMsdData;
        private FileOptions _settings;
        private int previousFileRefId = -1;
        private bool isDuplicatedFile = false;
        private string invalidMsd1ReturnErrorMsg = string.Empty;
        private StringBuilder headerErrors;
        private StringBuilder bodyErrorMsg;
        private Msd1FileProcess _msd1FileProcess;
        private Msd2FileProcess _msd2FileProcess;
        private Msd3FileProcess _msd3FileProcess;

        public GESMESFileProcess(
            FlatFile fileInfo,
            LogFileData logFileData,
            GesmesHelpers gesmesHelpers,
            ValidateMsdData validateMsdData,
            Msd1FileProcess msd1FileProcess,
            Msd2FileProcess msd2FileProcess,
            Msd3FileProcess msd3FileProcess,
            IMsd1DataService msd1DataService,
            IMsd2DataService msd2DataService,
            IMsd3DataService msd3DataService,
            IFileProcessService fileProcessService,
            IHelperService helperService,
            IOptions<FileOptions> settings,
            IEmailSender emailSender,
            ILogger<GESMESFileProcess> logger)
        {
            _fileInfo = fileInfo;
            _logFileData = logFileData;
            _gesmesHelpers = gesmesHelpers;
            _helperService = helperService;
            _msd1DataService = msd1DataService;
            _msd2DataService = msd2DataService;
            _msd3DataService = msd3DataService;
            _validateMsdData = validateMsdData;
            _fileProcessService = fileProcessService;
            _logger = logger;
            _settings = settings.Value;
            _emailSender = emailSender;
            _msd1FileProcess = msd1FileProcess;
            _msd2FileProcess = msd2FileProcess;
            _msd3FileProcess = msd3FileProcess;
            headerErrors = new StringBuilder();
            bodyErrorMsg = new StringBuilder();
        }

        public void InitiateGesmesFileProcess(string filename)
        {
            try
            {
                _fileInfo = new FlatFile();
                _logFileData = new LogFileData();

                using (StreamReader sr = new StreamReader(@"" + filename))
                {
                    _gesmesHelpers.GesmesFile = (sr.ReadToEnd());
                }

                if (string.IsNullOrEmpty(_gesmesHelpers.GesmesFile)) return;
                _fileInfo = ProcessHeader(filename, _gesmesHelpers);

                if (_fileInfo != null && !string.IsNullOrEmpty(_fileInfo.SenderId) && string.IsNullOrEmpty(headerErrors.ToString().Trim()))
                {
                    if (_fileInfo.IsAmendment.HasValue && _fileInfo.IsAmendment.Value == 1 && previousFileRefId > -1)
                    {
                        switch (_fileInfo.TableRef.ToUpper())
                        {
                            case "TABLE-1-UO-Q":
                            case "TABLE-1-UQ-Q":
                                _msd1DataService.DeleteAllPreviousMsd1Data(previousFileRefId);
                                break;
                            case "TABLE-2-FT-A":
                                _msd2DataService.DeleteAllPreviousMsd2Data(previousFileRefId);
                                break;
                            case "TABLE-3-LA-A":
                                _msd3DataService.DeleteAllPreviousMsd3Data(previousFileRefId);
                                break;
                        }
                    }

                    if ((!isDuplicatedFile && _fileInfo.IsAmendment.Value == 0)
                        || (_fileInfo.IsAmendment.Value == 1 && previousFileRefId > -1)
                    )
                    {
                        switch (_fileInfo.TableRef.ToUpper())
                        {
                            case "TABLE-1-UO-Q":
                            case "TABLE-1-UQ-Q":
                                var errorMassage = _msd1FileProcess.ProcessMsd1Records(_gesmesHelpers, filename, _fileInfo);
                                bodyErrorMsg.Append(errorMassage);
                                break;
                            case "TABLE-2-FT-A":
                                errorMassage = _msd2FileProcess.ProcessMsd2Records(_gesmesHelpers, filename, _fileInfo);
                                bodyErrorMsg.Append(errorMassage);
                                break;
                            case "TABLE-3-LA-A":
                                errorMassage = _msd3FileProcess.ProcessMsd3Records(_gesmesHelpers, filename, _fileInfo);
                                bodyErrorMsg.Append(errorMassage);
                                break;
                        }
                    }
                    else
                    {
                        var errmsg = (_fileInfo.IsAmendment.Value == 1 && previousFileRefId == -1) ?
                            string.Format("File {0} is an amendment file but there is no file with the same submission reference to amend - please contact the helpdesk for advice.", _fileInfo.FileName) :
                            string.Format("File {0}  has the same sender reference as a file you have already sent, so it has not been processed." +
                                          " If it is not a duplicate sent by mistake, please use a different sender reference in the header and resubmit the file.", _fileInfo.FileName);

                        bodyErrorMsg.AppendFormat(errmsg);
                    }

                    if (!string.IsNullOrEmpty(bodyErrorMsg.ToString()))
                    {
                        AddErrorLogToDbAndNotifyUser(filename, bodyErrorMsg.ToString());
                    }
                    bodyErrorMsg.Clear();
                }
                headerErrors.Clear();
            }
            catch (Exception err)
            {
                _logger.LogError("Gesmes File error:", err.Message, err.InnerException != null ? err.InnerException.Message : string.Empty, err.StackTrace);

                if (_fileInfo != null)
                {
                    bodyErrorMsg.AppendLine(filename + Environment.NewLine + err.Message);
                    AddErrorLogToDbAndNotifyUser(filename, bodyErrorMsg.ToString());
                }
            }
            finally
            {
                ArchiveProcessedFilesFromBucket(filename, false);
                bodyErrorMsg.Clear();
                headerErrors.Clear();
            }
        }

        private FlatFile ProcessHeader(string filename, GesmesHelpers gesmesHelpers)
        {
            var lines = File.ReadAllLines(filename);
      
            var senderID = gesmesHelpers.GetDataItem("UNOC:3");
            _fileInfo.SenderId = (senderID.Contains(":") ? senderID.Split(':')[0] : senderID);
            var isValidSenederId = !string.IsNullOrEmpty(_fileInfo.SenderId) ?
                string.Empty : "The Gemses file header record is missing or invalid. The file has not been processed. " + " Please correct and resubmit.";

            if (!string.IsNullOrEmpty(isValidSenederId))
            {
                headerErrors.AppendLine().AppendFormat(isValidSenederId);
                NotifyErrorMsgToUsers(_fileInfo, headerErrors.ToString());
                return null;
            }

            _fileInfo.FileName = filename;
            _fileInfo.TotalRecords = gesmesHelpers.GetNumberOfRecords();
            _fileInfo.TableRef = gesmesHelpers.GetDataItem("DSI");

            var validateMsdRecord = string.Empty;
            if (_fileInfo.TableRef.ToUpper().Equals("TABLE-1-UO-Q")
                                   || _fileInfo.TableRef.ToUpper().Equals("TABLE-1-UQ-Q")
                                   || _fileInfo.TableRef.ToUpper().Equals("TABLE-2-FT-A")
                                   || _fileInfo.TableRef.ToUpper().Equals("TABLE-3-LA-A")
                                   || _fileInfo.TableRef.ToUpper().Equals("TABLE-4-VS-Q")
                                   || _fileInfo.TableRef.ToUpper().Equals("TABLE-5-FT-A"))
            {
                if (!(_fileInfo.TableRef.ToUpper().Equals("TABLE-1-UO-Q")
                                    || _fileInfo.TableRef.ToUpper().Equals("TABLE-1-UQ-Q")
                                    || _fileInfo.TableRef.ToUpper().Equals("TABLE-2-FT-A")
                                    || _fileInfo.TableRef.ToUpper().Equals("TABLE-3-LA-A")))
                {
                    validateMsdRecord = "The Gesmes file header record is not in the correct format for an MSD1 submission. The file has not been processed. Please correct and resubmit.";
                    ArchiveProcessedFilesFromBucket(filename, true);
                    var tokens = new Dictionary<string, dynamic>();
                    tokens.Add("filename", filename);
                    _emailSender.SendEmail(_settings.MaritimeHelpdeskEmailAddress, "Error in file submitted", validateMsdRecord, tokens);
                    return null;
                }
            }
            else
            {
                validateMsdRecord = "The Gesmes file header record is not in the correct format for a submission. The file has not been processed. Please correct and resubmit.";
                headerErrors.AppendLine().AppendFormat(validateMsdRecord);
            }

            _fileInfo.IsAmendment = gesmesHelpers.GetAmmendment("STS");
            _fileInfo.CreationDate = DateTime.Now.Date;
            _fileInfo.IsTest = null;
            var regNo = gesmesHelpers.GetDataItem("UNB");
            _fileInfo.RegistrationNumber = regNo.Contains(":") ? int.Parse(regNo.Split(':')[1]) : int.Parse(regNo);
            _fileInfo.SendersRef = gesmesHelpers.GetDataItem("UNH");

            if (_fileInfo != null && string.IsNullOrEmpty(validateMsdRecord))
            {
                previousFileRefId = _fileProcessService.GetLastInvalidFileRefId(_fileInfo);
                isDuplicatedFile = _fileProcessService.CheckFileExist(_fileInfo);
                _fileProcessService.Add(_fileInfo);
            }

            if (string.IsNullOrEmpty(headerErrors.ToString().Trim())) return _fileInfo;
            AddErrorLogToDbAndNotifyUser(filename, headerErrors.ToString());
            return null;
        }

        private void ArchiveProcessedFilesFromBucket(string filename, bool isNonMsd1File)
        {
            string filebucket = _settings.GesmesBucket;
            string destinationBucket = isNonMsd1File ? _settings.PortGesmesBucket : _settings.ArchivedGesmesBucket;

            StorageClient storageClient = StorageClient.Create();
            foreach (var item in storageClient.ListObjects(filebucket))
            {
                if (!filename.Contains(item.Name)) continue;
                storageClient.CopyObject(filebucket, item.Name, destinationBucket, item.Name);
                storageClient.DeleteObject(filebucket, item.Name);
            }
            File.Delete(filename);
        }

        private void AddErrorLogToDbAndNotifyUser(string filename, string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage)) return;
            _logFileData.FileRefId = _fileInfo.FileRefId;
            _logFileData.DateTime = System.DateTime.Now;
            _logFileData.Description = errorMessage;
            _fileProcessService.AddLogFileData(_logFileData);
            NotifyErrorMsgToUsers(_fileInfo, errorMessage);
        }

        private void NotifyErrorMsgToUsers(FlatFile _fileInfo, string validateMsd1Record)
        {
            var tokens = new Dictionary<string, dynamic>
            {
                {"filename", _fileInfo.FileName},
                {"senderRef", _fileInfo.SendersRef},
                {"senderId", _fileInfo.SenderId}
            };
            var userName = _validateMsdData.GetUserName(_fileInfo);
            var userEmail = string.IsNullOrEmpty(userName) ? _settings.MaritimeHelpdeskEmailAddress : userName;
            tokens.Add("username", userEmail);

            _emailSender.SendEmail(_settings.MaritimeHelpdeskEmailAddress, "Error in file submitted", validateMsd1Record, tokens);
            if (!string.IsNullOrEmpty(userEmail))
            {
                _emailSender.SendEmail(userEmail, "Error in file submitted", validateMsd1Record, tokens);
            }
            else
            {
                var userMailServer = _fileProcessService.GetAllUsers(_fileInfo.SenderId);
                foreach (var email in userMailServer)
                {
                    _emailSender.SendEmail(email, "Error in file submitted", validateMsd1Record, tokens);
                }
            }
        }
    }
}
