using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Data;

namespace PortFreight.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        HtmlEncoder _htmlEncoder;
        JavaScriptEncoder _javaScriptEncoder;
        UrlEncoder _urlEncoder;
        private readonly UserManager<PortFreightUser> _userManager;

        public ConfirmEmailModel(UserManager<PortFreightUser> userManager, HtmlEncoder htmlEncoder,
                             JavaScriptEncoder javascriptEncoder,
                             UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _htmlEncoder = htmlEncoder;
            _javaScriptEncoder = javascriptEncoder;
            _urlEncoder = urlEncoder;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_urlEncoder.Encode(userId)}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Error confirming email for user with ID '{_urlEncoder.Encode(userId)}':");
            }

            return Page();
        }
    }
}
