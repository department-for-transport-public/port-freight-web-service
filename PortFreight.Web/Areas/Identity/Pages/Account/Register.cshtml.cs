using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Services.EmailSender;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<PortFreightUser> _signInManager;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        UrlEncoder _urlEncoder;
        HtmlEncoder _htmlEncoder;
        public RegisterModel(
            UserManager<PortFreightUser> userManager,
            SignInManager<PortFreightUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            UrlEncoder urlEncoder, HtmlEncoder htmlEncoder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _urlEncoder = urlEncoder;
            _htmlEncoder = htmlEncoder;
            Input = new InputModel();
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {

            [EmailAddress]
            [Display(Name = "Email address")]
            public string Email { get; set; }


            [DataType(DataType.Text)]
            [Display(Name = "Sender Id")]
            public string SenderId { get; set; }

            [Required(ErrorMessage = "The password field is required")]
            [StringLength(100, ErrorMessage = "The password must be at least {2} characters long", MinimumLength = 12)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
            public string ConfirmPassword { get; set; }

            [Required (ErrorMessage = "You must accept the terms of use")]
            [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms of use")]
            public bool AgreeToTerms { get; set; }
        }

        public void OnGet()
        {
            PopulatePortFreightUser();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            PopulatePortFreightUser();
            if (ModelState.IsValid)
            {
                var user = new PortFreightUser { UserName = Input.Email, Email = Input.Email, SenderId = Input.SenderId };
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: "https");

                    _emailSender.SendEmail(Input.Email, "Confirm your email", _htmlEncoder.Encode(callbackUrl));

                    return RedirectToPage("RegisterConfirmation");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private void PopulatePortFreightUser()
        {
            var applicationUser = TempData.GetKeep<PortFreightUser>("PreApprovedUser");
            if (applicationUser != null)
            {
                Input.Email = applicationUser.Email;
                Input.SenderId = applicationUser.SenderId;
            }
        }
    }
}
