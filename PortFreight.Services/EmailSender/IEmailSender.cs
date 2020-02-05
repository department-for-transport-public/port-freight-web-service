using System;
using System.Collections.Generic;
using System.Text;

namespace PortFreight.Services.EmailSender
{
    public interface IEmailSender
    {
        bool SendEmail(string email, string subject, string htmlMessage = "", Dictionary<string, dynamic> tokens = null);
    }
}
