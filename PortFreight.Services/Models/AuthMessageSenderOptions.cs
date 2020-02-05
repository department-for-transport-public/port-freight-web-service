using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Services.Models
{
    public class AuthMessageSenderOptions
    {
        public string VerifyEmailTemplate { get; set; }
        public string ResetPasswordTemplate { get; set; }
        public string NotifyKey { get; set; }
        public string InviteByColleagueTemplate { get; set; }
        public string SubmissionFailureTemplate { get; set; }        
    }
}
