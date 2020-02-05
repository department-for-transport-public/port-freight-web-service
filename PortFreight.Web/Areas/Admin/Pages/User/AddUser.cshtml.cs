using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.User;
using PortFreight.Services.EmailSender;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;


namespace PortFreight.Web.Areas.Admin.Pages
{
    public class AddUserModel : PageModel
    {
        [BindProperty]
        [EmailAddress]
        [Required(ErrorMessage = "Enter an Email address")]
        public string Email { get; set; }
        
        [BindProperty]
        [Required(ErrorMessage = "Enter a Sender ID")]
        public string SenderID { get; set; }
        public bool ResendInvite = false;
        public bool Saved;

        private readonly ILogger<AddUserModel> _logger;
        private readonly UserDbContext _userContext;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;

        public AddUserModel(
                ILogger<AddUserModel> logger,
                UserDbContext userContext,
                UserManager<PortFreightUser> userManager,
                IUserService userService,
                IEmailSender emailSender
            )
        {
            _logger = logger;
            _userContext = userContext;
            _userService = userService;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostAddUser()
        {
            try
            {
                if (_userService.IsUserRegistered(Email))
                {
                    ModelState.AddModelError("Email", "This email is registered and active, cannot change status here");
                    return Page();
                }

            bool isValid = IsValidEmail(Email);

                if(!isValid)
                {
                    ModelState.AddModelError("Email", "Enter a valid email");
                    return Page();
                }

                if (SenderID == null)
                {
                    ModelState.AddModelError("SenderID", "Enter a Sender Id");
                }
                
                else
                {
                    addNewPreApprovedUserAsync();
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", "There has been a problem. Please try again.");
                _logger.LogError(e.Message);
            }

            if (SendEmail())
            {
                ModelState.AddModelError("Success", "Invite email has been sent.");
            }
            else
            {
                ModelState.AddModelError("CustomError", "There has been a problem sending the invite email");
            }

            return Page();
        }

        private bool SendEmail()
        {
            var callbackUrl = Url.Page(
                    "/Account/PreRegister",
                    pageHandler: null,
                    values: new { area = "Identity" },
                    protocol: "https");

            var content = new Dictionary<string, dynamic>
                {
                    { "userId", "maritimestats.help@dft.gov.uk" },
                    { "senderId", SenderID }
                };

            return _emailSender.SendEmail(Email, "Invite a Colleague", callbackUrl, content);
        }

        private async void addNewPreApprovedUserAsync()
        {
            var preAprovedUser = new PreApprovedUser
            {
                EmailAddress = Email,
                SenderId = SenderID
            };

            await _userContext.PreApprovedUsers.AddAsync(preAprovedUser);
            await _userContext.SaveChangesAsync();
        }
       
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                    RegexOptions.None, TimeSpan.FromMilliseconds(200));

                string DomainMapper(Match match)
                {
                    var idn = new IdnMapping();

                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    

        public IActionResult OnPostResendInvite()
        {
            if (SendEmail())
            {
                ModelState.AddModelError("Success", "Invite email has been resent.");
            }
            else
            {
                ModelState.AddModelError("CustomError", "There has been a problem sending the invite email");
            }

            return Page();
        }
    }
}
