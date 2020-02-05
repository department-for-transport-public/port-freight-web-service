using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Pages
{
    public class DashboardModel : BaseMsd1PageModel
    {
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly PortFreightContext _portFreightContext;
        public DashboardModel(UserManager<PortFreightUser> userManager, PortFreightContext portFreightContext)
        {
            _userManager = userManager;
            _portFreightContext = portFreightContext;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return RedirectToPage("/Account/Logout", new { area = "Identity" });
            }

            if (!_portFreightContext.ContactDetails.Any(x => x.SenderId == user.SenderId))
            {
                return RedirectToPage("/Profile/ContactDetails");
            }

            if (!_portFreightContext.SenderType.Any(x => x.SenderId == user.SenderId))
            {
                return RedirectToPage("/Profile/RespondentDetails");
            }

            var senderType = _portFreightContext.SenderType
                                .Where(x => x.SenderId == user.SenderId)
                                .FirstOrDefault();

            ViewBag.IsPort = senderType.IsPort;
            ViewBag.IsAgentOrLine = senderType.IsAgent || senderType.IsLine;

            return Page();
        }
    }
}