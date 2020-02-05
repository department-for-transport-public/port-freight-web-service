using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Services.EmailSender;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<PortFreightUser> _signInManager;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly PortFreightContext _portFreightContext;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        public string VerifyEmailUrl;
        public int LogInCount = 0;

        public LoginModel(SignInManager<PortFreightUser> signInManager,
            ILogger<LoginModel> logger,
            IConfiguration config,
            UserManager<PortFreightUser> userManager,
            PortFreightContext portFreightContext,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _logger = logger;
            _config = config;
            _userManager = userManager;
            _portFreightContext = portFreightContext;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Enter your email address")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Enter your password")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                await _userManager.UpdateSecurityStampAsync(user);
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded && user != null)
                {
                    _logger.LogInformation(user.Id + " logged in.");

                    var skipProfileIds = _config["SkipProfile:SenderId"];
                    if (skipProfileIds != null && skipProfileIds.Contains(user.SenderId))
                    {
                        return RedirectToPage("/ApiKey/ApiKey");
                    }
                    if (!(_portFreightContext.ContactDetails.Any(x => x.SenderId == user.SenderId)))
                    {
                        return RedirectToPage("/Profile/ContactDetails");
                    }
                    if (!(_portFreightContext.SenderType.Any(x => x.SenderId == user.SenderId)))
                    {
                        return RedirectToPage("/Profile/RespondentDetails");
                    }
                    return RedirectToPage("/Dashboard");
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning(user.Id + " account locked out.");
                    return RedirectToPage("./Lockout");
                }
                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("EmailNotConfirmed", "Verify your email address by clicking the link in your email");
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    VerifyEmailUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: "https");
                    return Page();
                }
                if (user == null)
                {

                    if (TempData["LogInAttempts"] != null)
                    {
                        LogInCount = (int)TempData["LogInAttempts"];
                        TempData.Remove("LogInAttempts");
                    }
                    if (LogInCount < 4)
                    {
                        TempData.Add("LogInAttempts", LogInCount += 1);
                    }
                    else
                    {
                        return RedirectToPage("./Lockout");
                    }
                }
            }

            ModelState.AddModelError("Input_Email", "Sorry, we did not recognise your sign-in details, please try again.");
            return Page();
        }

        public IActionResult OnPostResendEmail(string callback)
        {
            try
            {
                _emailSender.SendEmail(Input.Email, "Confirm your email", callback);
                var user = new PortFreightUser
                {
                    Email = Input.Email
                };
                TempData.Put("PreApprovedUser", user);
                return RedirectToPage("./RegisterConfirmation");
            }
            catch
            {
                return Page();
            }
        }
    }
}