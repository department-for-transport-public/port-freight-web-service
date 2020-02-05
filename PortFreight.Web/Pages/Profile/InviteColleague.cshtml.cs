using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.EmailSender;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Pages.Profile
{
    public class InviteColleagueModel : PageModel
    {
        private readonly ILogger<InviteColleagueModel> _logger;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly UserDbContext _userContext;
        private readonly IEmailSender _emailSender;
        public List<PreApprovedUser> preApprovedUsers;
        public List<InvitedColleagueViewMidel> invitedUsers;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public InviteColleagueModel(
           UserManager<PortFreightUser> userManager,
           UserDbContext userContext,
        ILogger<InviteColleagueModel> logger,
           IEmailSender emailSender)
        {
            _userManager = userManager;
            _userContext = userContext;
            _logger = logger;
            _emailSender = emailSender;
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public class InvitedColleagueViewMidel
        {
            public string Email { get; set; }
            public string CurrentStatus { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            PreApprovedUserList(user);

            return Page();

        }
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return RedirectToPage("/Account/Logout", new { area = "Identity" });
            }
            bool isAlreadyRegistered = _userContext.Users.Any(x => x.Email == Input.Email);
            if (isAlreadyRegistered)
            {
                ModelState.AddModelError("Input.Email", "This email is already registered");
                PreApprovedUserList(user);
                return Page();
            }
            if (!ModelState.IsValid)
            {
                PreApprovedUserList(user);
                return Page();
            }

            var callbackUrl = Url.Page(
                        "/Account/PreRegister",
                        pageHandler: null,
                        values: new { area = "Identity" },
                        protocol: "https");

            var content = new Dictionary<string, dynamic>
                            {
                                { "userId", user.Email },
                                { "senderId", user.SenderId }
                            };
            _emailSender.SendEmail(Input.Email, "Invite a Colleague", callbackUrl, content);

            var preAprovedUser = new PreApprovedUser
            {
                EmailAddress = Input.Email,
                SenderId = user.SenderId
            };
            await _userContext.PreApprovedUsers.AddAsync(preAprovedUser);
            await _userContext.SaveChangesAsync();

            TempData["inviteduser"] = Input.Email.ToString();
            return RedirectToPage("./ConfirmInviteColleague");
        }

        public void PreApprovedUserList(PortFreightUser user)
        {
            if (preApprovedUsers == null)
            {
                invitedUsers = new List<InvitedColleagueViewMidel>();
                var registeredUsers = _userContext.Users
                    .Where(x => x.SenderId == user.SenderId);

                var registeredEmails = registeredUsers.Select(x => x.Email);

                var allPreapprovedUsers = _userContext.PreApprovedUsers
                    .Where(x => x.SenderId == user.SenderId)
                    .Where(x => x.EmailAddress.Contains('@'))
                    .GroupBy(x => x.EmailAddress)
                    .Select(x => x.FirstOrDefault())
                    .Select(x => x.EmailAddress)
                    .Union(registeredEmails)
                    .Distinct();

                foreach (string invitedUser in allPreapprovedUsers)
                {
                    bool registered = registeredUsers.Any(x => x.Email == invitedUser);
                    invitedUsers.Add(new InvitedColleagueViewMidel
                    {
                        Email = invitedUser,
                        CurrentStatus = registered == true ? "active" : "invited"
                    });
                }
            }
        }
    }

}