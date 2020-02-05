using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.User;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class PreRegisterModel : PageModel
    {
        private readonly UserDbContext _usercontext;
        private readonly IUserService _userService;

        public PreRegisterModel(UserDbContext usercontext, IUserService userservice)
        {
            _usercontext = usercontext;
            _userService = userservice;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Enter your email address")]
            [EmailAddress]
            [Display(Name = "Email address")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Enter your sender id")]
            [DataType(DataType.Text)]
            [Display(Name = "Sender ID")]
            public string SenderId { get; set; }
        }
               

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                var user = new PreApprovedUser { EmailAddress = Input.Email, SenderId = Input.SenderId };

                if (EmailIdRegistered(user))
                {
                    ModelState.AddModelError("EmailAlreadyRegistered", "This email is already registered");
                    return Page();
                }
             
                var isUserPreApproved = _usercontext.PreApprovedUsers.Any(x => x.EmailAddress == user.EmailAddress);

                if (isUserPreApproved)
                {
                    if (DeatailsMatchPreApprovedList(user))
                    {
                        var preApprovedUser = new PortFreightUser { UserName = Input.Email.ToLower(), Email = Input.Email.ToLower(), SenderId = Input.SenderId.ToUpper() };
                        TempData.Put("PreApprovedUser", preApprovedUser);
                        return RedirectToPage("Register");                           
                    }

                    ModelState.AddModelError("CheckDetailsEntered", "Please check the details entered match those in your invitation email");
                    return Page();
                }
                   
                ModelState.AddModelError("CheckDetailsEntered", "We are unable to validate your details, please contact helpdesk");
                return Page();  
            }

            return Page();
        }

        private bool DeatailsMatchPreApprovedList(PreApprovedUser user)
        {
            return _usercontext.PreApprovedUsers.Any(s => s.EmailAddress == user.EmailAddress && s.SenderId == user.SenderId);
        }

        private bool EmailIdRegistered(PreApprovedUser user)
        {
            return _usercontext.Users.Any(x => x.Email == user.EmailAddress);
        }
    }
}
