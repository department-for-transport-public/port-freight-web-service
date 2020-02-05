

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Client;
using PortFreight.Services.EmailSender;
using PortFreight.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortFreight.Services.Areas.Identity.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IOptions<AuthMessageSenderOptions> _config;
        private readonly ILogger _logger;
        public AuthMessageSenderOptions AuthMessageSenderOptions { get; set; }
        public string EmailTemplate { get; set; }
        public Dictionary<String, dynamic> TemplateContent { get; set; }

        public EmailSender(IOptions<AuthMessageSenderOptions> config, ILogger<IEmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public bool SendEmail(string email, string subject, string message= "", Dictionary<string,dynamic> tokens = null)
        {
            AuthMessageSenderOptions = _config.Value;

            var client = new NotificationClient(AuthMessageSenderOptions.NotifyKey);

            ConfigureEmailTemplate(email, subject, message, tokens);

            try
            {
                client.SendEmail(email, EmailTemplate, TemplateContent, null, null);
            }
            catch (Notify.Exceptions.NotifyClientException e)
            {
                _logger.LogError(email + e.Message);
                return false;
            }

            return true;
        }

        private void ConfigureEmailTemplate(string email, string subject, string message="", Dictionary<string,dynamic> tokens = null)
        {
            switch (subject)
            {
                case "Confirm your email":
                    EmailTemplate = AuthMessageSenderOptions.VerifyEmailTemplate;
                    TemplateContent = new Dictionary<string, dynamic>
                            {
                                { "magic_url", message },
                                { "name", email },
                                { "email address", email }
                            };
                    break;
                case "Reset Password":
                    EmailTemplate = AuthMessageSenderOptions.ResetPasswordTemplate;
                    TemplateContent = new Dictionary<string, dynamic>
                            {
                                { "reset_url", message },
                                { "name", email },
                                { "email address", email }
                            };
                    break;
                case "Error in file submitted":
                    {
                        EmailTemplate = AuthMessageSenderOptions.SubmissionFailureTemplate;
                        TemplateContent = new Dictionary<string, dynamic>
                                {
                                    {"email address",email },
                                    {"username", tokens == null? string.Empty : tokens.GetValueOrDefault("username") },
                                    {"sender_reference", tokens == null? string.Empty : tokens.GetValueOrDefault("senderRef") },
                                    {"senderId", tokens == null? string.Empty : tokens.GetValueOrDefault("senderId") },
                                    {"date_return_submitted", DateTime.Now.Date.ToString("dd/MM/yyyy") },
                                    {"filename", tokens == null? string.Empty : tokens.GetValueOrDefault("filename") },                                    
                                    {"error line and  description", message }
                                };
                    }
                    break;
                case "Invite a Colleague":
                    EmailTemplate = AuthMessageSenderOptions.InviteByColleagueTemplate;
                    TemplateContent = new Dictionary<string, dynamic>
                            {
                                { "magic link", message },
                                { "email", email },
                                { "email address", tokens["userId"] },
                                { "Sender ID", tokens["senderId"] }
                            };
                    break;
                default:
                    EmailTemplate = AuthMessageSenderOptions.VerifyEmailTemplate;
                    TemplateContent = new Dictionary<string, dynamic>
                            {
                                { "reset_url", message },
                                { "name", email },
                                { "email address", email }
                            };
                    break;
            }
        }
    }
}