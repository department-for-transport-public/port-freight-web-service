using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Pages.Profile
{

    public class PortDetailsModel : BaseMsd1PageModel
    {
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly PortFreightContext _context;
        private readonly IHelperService _helperService;

        public SenderPort SenderPortItem { get; set; }
        public RespondentViewModel RespondentVM { get; set; }
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public List<string> UkPorts { get; set; }
        public string SuccessMessage { get; set; }

        public SenderIdPort SenderIdPort { get; set; }

        public PortDetailsModel(UserManager<PortFreightUser> userManager, PortFreightContext context, IHelperService helperService)
        {
            _userManager = userManager;
            _context = context;
            _helperService = helperService;
        }

        public class InputModel
        {
            [Display(Name = "Port")]
            public string SenderPortLocode { get; set; }
            public bool SenderType = false;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return RedirectToPage("/Account/Logout", new { area = "Identity" });
            }

            InitialisePage(user);

            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string locode)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Input.SenderPortLocode = locode;
            if (Input.SenderPortLocode == null)
            {
                ModelState.AddModelError("Input.SenderPortLocode", "Enter the port name or locode that you report data for");
            }
            var portLocode = _helperService.GetPortCodeByPort(locode);
            if (portLocode == null)
            {
                ModelState.AddModelError("Input.SenderPortLocode", "Enter a valid port name or locode that you report data for");
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (_context.SenderIdPort.Where(p => p.Locode == portLocode && p.SenderId == user.SenderId).Count() > 0)
            {
                ModelState.AddModelError("Input.SenderPortLocode", "Port is already in the list");
            }

            if (ModelState.IsValid)
            {
                var senderIdPort = new SenderIdPort
                {
                    Locode = portLocode,
                    SenderId = user.SenderId
                };

                _context.SenderIdPort.Add(senderIdPort);
                await _context.SaveChangesAsync();
                SuccessMessage = "Port successfully saved";
            }

            InitialisePage(user);

            return Page();
        }

        public async Task OnPostRemovePort(string Id)
        {
            ModelState.Clear();

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (Id != null && Id != string.Empty)
            {
                var senderTypeID = _context.SenderIdPort.Where(x => x.SenderId == user.SenderId && x.Locode == Id).First();
                _context.SenderIdPort.Remove(senderTypeID);
                await _context.SaveChangesAsync();
            }
            InitialisePage(user);
            SuccessMessage = "Port successfully deleted";
        }

        private SenderType FindSavedPort(PortFreightUser user)
        {
            return _context.SenderType.Include(p => p.SenderIdPort)
                .Where(p => p.SenderId == user.SenderId)
                .Include(p => p.SenderIdPort)
                .SingleOrDefault();
        }
        private void AssignDataToInput(SenderType senderType)
        {
            RespondentVM.PortList.Clear();
            foreach (var senderPort in senderType.SenderIdPort)
            {
                SenderPort port = new SenderPort
                {
                    Id = Guid.NewGuid(),
                    SenderId = senderPort.SenderId,
                    Locode = senderPort.Locode,
                    PortName = _context.GlobalPort.Where(s => s.Locode == senderPort.Locode)
                    .Select(s => s.PortName).FirstOrDefault()
                };
                RespondentVM.SenderId = senderPort.SenderId;
                RespondentVM.PortList.Add(port);
            };

        }
        private void InitialisePage(PortFreightUser user)
        {
            Input.SenderType = _context.SenderType
             .Where(x => x.SenderId == user.SenderId)
             .FirstOrDefault().IsPort;

            UkPorts = _helperService.GetRespondentPorts();
            RespondentVM = new RespondentViewModel();
            var existingPort = FindSavedPort(user);
            if (existingPort != null)
            {
                AssignDataToInput(existingPort);
            }
        }
    }
}