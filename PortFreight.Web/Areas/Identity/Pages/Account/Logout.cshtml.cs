using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PortFreight.Data;

namespace PortFreight.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<PortFreightUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly UserManager<PortFreightUser> _userManager;

        public LogoutModel(SignInManager<PortFreightUser> signInManager, ILogger<LogoutModel> logger, UserManager<PortFreightUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPost(string returnUrl = null)
        {
            if (User?.Identity.IsAuthenticated == true)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                await _signInManager.SignOutAsync();
                await _userManager.UpdateSecurityStampAsync(user);

                foreach (var key in HttpContext.Request.Cookies.Keys)
                {
                    HttpContext.Response.Cookies.Append(key, "", new CookieOptions() { Expires = DateTime.Now.AddDays(-1) });
                }

                foreach (var cookie in HttpContext.Request.Cookies.Keys)
                {
                    if (cookie == ".AspNetCore.Session" || cookie == ".AspNetCore.Identity.Application")
                        HttpContext.Response.Cookies.Delete(cookie);
                }

                _logger.LogInformation("User logged out.");
            }
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }

        }
    }
}