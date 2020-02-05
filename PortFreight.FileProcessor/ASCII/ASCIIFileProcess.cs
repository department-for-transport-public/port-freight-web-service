using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortFreight.Data.Entities;
using PortFreight.FileProcess.Common;
using PortFreight.FileProcessor.ASCII;
using PortFreight.Services.EmailSender;
using PortFreight.Services.FileProcess;
using PortFreight.Services.MSD1;
using PortFreight.Services.MSD2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace PortFreight.FileProcess.ASCII
{
    public class ASCIIFileProcess
    {
        private readonly ILogger _logger;
        private readonly IFileProcessService _fileProcessService;
        private readonly IMsd1DataService _msd1DataService;
        private readonly IMsd2DataService _msd2DataService;
        private readonly IMsd3DataService _msd3DataService;
        private readonly IEmailSender _emailSender;
        private LogFileData _logFileData;
        private FlatFile _fileInfo;
        private MSD1FileProcess _msd1FileProcess;
        private MSD2FileProcess _msd2FileProcess;
        private MSD3FileProcess _msd3FileProcess;
        private ValidateMsdData _validateMsdData;
        private FileOptions _settings;
        private StringBuilder headerErrors;
        private StringBuilder bodyErrorMsg;
        private int previousFileRefId = -1;
        private bool isDuplicatedFile = false;
        private DateTime dateValue;

        public ASCIIFileProcess(
                              FlatFile fileInfo,
                              MSD1FileProcess msd1FileProcess,
                              MSD2FileProcess msd2FileProcess,
                              MSD3FileProcess msd3FileProcess,
                              LogFileData logFileData,
                               ValidateMsdData validateMsdData,
                              IEmailSender emailSender,
                              IFileProcessService fileProcessService,
                              IMsd1DataService msd1DataService,
                              IMsd2DataService msd2DataService,
                              IMsd3DataService msd3DataService,
                              ILogger<ASCIIFileProcess> logger,
                              IOptions<FileOptions> settings
                              )
        {
            _fileInfo = fileInfo;
            _logFileData = logFileData;
            _msd1FileProcess = msd1FileProcess;
            _msd2FileProcess = msd2FileProcess;
            _msd3FileProcess = msd3FileProcess;
            _validateMsdData = validateMsdData;
            _fileProcessService = fileProcessService;
            _msd1DataService = msd1DataService;
            _msd2DataService = msd2DataService;
            _msd3DataService = msd3DataService;
            _emailSender = emailSender;
            _logger = logger;
            _settings = settings.Value;
            headerErrors = new StringBuilder();
            bodyErrorMsg = new StringBuilder();
        }

        public void InitiateAsciiFileProcess(string filename)
        {
            try
            {
                _fileInfo = new FlatFile();
                _logFileData = new LogFileData();
                var senderId = ProcessFileHeader(filename);

                if (!string.IsNullOrEmpty(senderId) && string.IsNullOrEmpty(headerErrors.ToString().Trim()))
                {   
                    if (_fileInfo != null
                        && _fileInfo.IsAmendment.HasValue
                        && _fileInfo.IsAmendment.Value == 1
                        && previousFileRefId > -1)
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
                        || (_fileInfo.IsAmendment.Value == 1 && previousFileRefId > -1))
                    {
                        switch (_fileInfo.TableRef.ToUpper())
                        {
                            case "TABLE-1-UO-Q":
                            case "TABLE-1-UQ-Q":
                                var errorMassage = _msd1FileProcess.ProcessMsd1Records(filename, _fileInfo);
                                bodyErrorMsg.Append(errorMassage);
                                break;
                            case "TABLE-2-FT-A":
                                errorMassage = _msd2FileProcess.ProcessMsd2Records(filename, _fileInfo);
                                bodyErrorMsg.Append(errorMassage);
                                break;
                            case "TABLE-3-LA-A":
                                errorMassage = _msd3FileProcess.ProcessMsd3Records(filename, _fileInfo);
                                bodyErrorMsg.Append(errorMassage);
                                break;
                        }
                    }
                    else
                    {
                        var errmsg = (_fileInfo.IsAmendment.Value == 1 && previousFileRefId == -1) ?
                                                string.Format("{0} is an amendment file but there is no file with the same submission reference to amend - please contact the helpdesk for advice.", _fileInfo.FileName.Substring(0, _fileInfo.FileName.IndexOf('.') + 4)) :
                                                    string.Format("{0} has the same sender reference as a file you have already sent, so it has not been processed." +
                                                                " If it is not a duplicate sent by mistake, please use a different sender reference in the header and resubmit the file.", _fileInfo.FileName.Substring(0, _fileInfo.FileName.IndexOf('.') + 4));
                        bodyErrorMsg.AppendFormat(errmsg);

                    }
                    if (!string.IsNullOrEmpty(bodyErrorMsg.ToString().Trim()))
                    {
                        AddErrorLogToDbAndNotifyUser(filename, bodyErrorMsg.ToString());
                    }
                    bodyErrorMsg.Clear();
                }
                headerErrors.Clear();
            }
            catch (Exception err)
            {
                _logger.LogError("Ascii File error:", err.Message, err.InnerException != null ? err.InnerException.Message : string.Empty, err.StackTrace);

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
                _fileInfo = null;
            }
        }

        private void AddErrorLogToDbAndNotifyUser(string filename, string errorMessage)
        {
            if (!(string.IsNullOrEmpty(errorMessage)))
            {
                _logFileData.FileRefId = _fileInfo.FileRefId;
                _logFileData.DateTime = System.DateTime.Now;
                _logFileData.Description = errorMessage;
                _logFileData.IsEmailed = true;
                _fileProcessService.AddLogFileData(_logFileData);
                NotifyErrorMsgToUsers(_fileInfo, errorMessage);
            }
        }

        private string ProcessFileHeader(string filename)
        {
            var header = File.ReadAllLines(filename)[0];
     
            var isemptyFile = !(string.IsNullOrEmpty(header) || header.Length < 18) ? string.Empty : "The Ascii file header record is missing or invalid. The file has not been processed. Please correct and resubmit.";
            if (!string.IsNullOrEmpty(isemptyFile))
                headerErrors.AppendLine().AppendFormat(isemptyFile);

            _fileInfo.FileName = filename;
            _fileInfo.SenderId = header.Length < 18 ? _fileProcessService.GetSenderIdByFileName(filename) : header.Substring(12, 6);

            dateValue = DateTime.Now.Date;
            var dateString = header.Length < 26 ? string.Empty : header.Substring(18, 2) + "/" + header.Substring(20, 2) + "/" + header.Substring(22, 4);
            DateTime.TryParseExact(dateString, "dd/MM/yyyy", new CultureInfo("en-GB"),
                                DateTimeStyles.None, out dateValue);
            _fileInfo.CreationDate = dateValue;

            Int16 totalRecord = 0;
            var strTotalRecord = header.Length < 32 ? string.Empty : header.Substring(26, 6);
            Int16.TryParse(strTotalRecord, out totalRecord);
            _fileInfo.TotalRecords = totalRecord;


            _fileInfo.TableRef = header.Length < 44 ? string.Empty : header.Substring(32, 12);

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
                    validateMsdRecord = "The Ascii file header record is not in the correct format for a submission. The file has not been processed.";
                    ArchiveProcessedFilesFromBucket(filename, true);
                    var tokens = new Dictionary<string, dynamic>();
                    tokens.Add("filename", filename);
                    _emailSender.SendEmail(_settings.MaritimeHelpdeskEmailAddress, "Error in file submitted", validateMsdRecord, tokens);
                    return string.Empty;
                }
            }
            else
            {
                validateMsdRecord = "The Ascii file header record is not in the correct format for a submission. The file has not been processed. Please correct and resubmit.";
                headerErrors.AppendLine().AppendFormat(validateMsdRecord);
            }


            _fileInfo.IsAmendment = header.Length < 45 || header.Substring(44, 1).ToUpper().Equals("Y") ? Convert.ToSByte(1) : Convert.ToSByte(0);
            _fileInfo.IsTest = header.Length < 46 || header.Substring(45, 1).ToUpper().Equals("Y") ? Convert.ToSByte(1) : Convert.ToSByte(0);


            int regNumber = 0;
            var strRegNumber = header.Length < 50 ? string.Empty : header.Substring(46, 4);
            Int32.TryParse(strRegNumber, out regNumber);
            _fileInfo.RegistrationNumber = regNumber;

            _fileInfo.SendersRef = header.Length < 62 ? string.Empty : header.Substring(50, 12);

            if (_fileInfo != null)
            {
                previousFileRefId = _fileProcessService.GetLastInvalidFileRefId(_fileInfo);

                isDuplicatedFile = _fileProcessService.CheckFileExist(_fileInfo);

                _fileProcessService.Add(_fileInfo);
            }

            if (!string.IsNullOrEmpty(headerErrors.ToString().Trim()))
            {
                AddErrorLogToDbAndNotifyUser(filename, headerErrors.ToString());
            }
            return _fileInfo.SenderId;
        }

        private void NotifyErrorMsgToUsers(FlatFile _fileInfo, string validateMsd1Record)
        {
            var userName = _validateMsdData.GetUserName(_fileInfo);
            var userEmail = String.IsNullOrEmpty(userName) ? _settings.MaritimeHelpdeskEmailAddress : userName;
            var tokens = new Dictionary<string, dynamic>();
            tokens.Add("filename", _fileInfo.FileName.Substring(0, _fileInfo.FileName.IndexOf('.') + 4));
            tokens.Add("senderRef", _fileInfo.SendersRef);
            tokens.Add("senderId", _fileInfo.SenderId);
            tokens.Add("username", String.IsNullOrEmpty(userEmail) ? _settings.MaritimeHelpdeskEmailAddress : userEmail);

            _emailSender.SendEmail(_settings.MaritimeHelpdeskEmailAddress, "Error in file submitted", validateMsd1Record, tokens);
            if (!string.IsNullOrEmpty(userEmail))
            {
                _emailSender.SendEmail(userEmail, "Error in file submitted", validateMsd1Record, tokens);
            }
            else
            {
                var useremails = _fileProcessService.GetAllUsers(_fileInfo.SenderId);
                foreach (var email in useremails)
                {
                    _emailSender.SendEmail(email, "Error in file submitted", validateMsd1Record, tokens);
                }
            }
        }

        private void ArchiveProcessedFilesFromBucket(string filename, bool isNonMsd1File)
        {
            string filebucket = _settings.AsciiBucket;
            string destinationBucket = isNonMsd1File ? _settings.PortAsciiBucket : _settings.ArchivedAsciiBucket;

            StorageClient storageClient = StorageClient.Create();

            foreach (var Item in storageClient.ListObjects(filebucket))
            {
                if (filename.Contains(Item.Name))
                {
                    storageClient.CopyObject(filebucket, Item.Name, destinationBucket, Item.Name);
                    storageClient.DeleteObject(filebucket, Item.Name);
                }
            }
            File.Delete(filename);
        }
    }
}
