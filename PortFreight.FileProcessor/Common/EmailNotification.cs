using PortFreight.Services.EmailSender;
using PortFreight.Services.FileProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortFreight.FileProcessor.Common
{
    public class EmailNotification
    {
        private readonly IFileProcessService _fileProcessService;
        private readonly IEmailSender _emailSender;
        private StringBuilder invalidFileErrorMsg;

        public EmailNotification(IFileProcessService fileProcessService, IEmailSender emailSender)
        {
            _fileProcessService = fileProcessService;
            _emailSender = emailSender;
            invalidFileErrorMsg = new StringBuilder();
        }
        public void SendErrorNotificationToUsers()
        {   
            var senderIds = _fileProcessService.GetAllSenderIds();
            foreach (var senderId in senderIds)
            {
                var loggedDbDescription = _fileProcessService.GetLoggedFileData(senderId);
                loggedDbDescription.ForEach(x => invalidFileErrorMsg.AppendLine(x.Description));

                var useremails = _fileProcessService.GetAllUsers(senderId);
                foreach (var email in useremails)
                {
                    _emailSender.SendEmail(email, "Error in file submitted", invalidFileErrorMsg.ToString());
                    loggedDbDescription.ForEach(x => x.IsEmailed = true);
                    loggedDbDescription.ForEach(x => _fileProcessService.UpdateLogFileData(x));
                }
                invalidFileErrorMsg.Clear();
                loggedDbDescription.Clear();
            }
        }
    }
}
