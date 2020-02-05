using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Internal;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Pages.Msd1
{
    public class CompanyDetailsModel : BaseMsd1PageModel
    {
        public MSD1 MSD1 { get; set; }
        private readonly PortFreightContext _context;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly IHelperService _helperService;

        [BindProperty]
        public bool FromSummary { get; set; }
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public IList<string> AgentSenders { get; set; }
        public IList<string> LineSenders { get; set; }

        public PortFreightUser loggedInUser { get; set; }

        public CompanyDetailsModel(
            PortFreightContext context,
            IHelperService helperService,
            UserManager<PortFreightUser> userManager)
        {
            _context = context;
            _helperService = helperService;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            InitialisePage();

            loggedInUser = await _userManager.GetUserAsync(HttpContext.User);
            if (loggedInUser == null || loggedInUser.SenderId == null)
            {
                return RedirectToPage("/Account/Logout", new { area = "Identity" });
            }

            var senderTypesDetails = _helperService.GetSenderType(loggedInUser.SenderId);
            var AgentId = !CustomExtensions.NotNullOrEmpty(Input.AgentSenderId) && senderTypesDetails.IsAgent == true
                ? loggedInUser.SenderId : Input.AgentSenderId;
            var LineId = !CustomExtensions.NotNullOrEmpty(Input.LineSenderId) && senderTypesDetails.IsLine == true
                ? loggedInUser.SenderId : Input.LineSenderId;

            if (LineId != null)
            {
                Input.LineSenderId = _context.ContactDetails.Where(s => s.SenderId == LineId).Select
                (
                    s =>
                    s.SenderId.ToString() +
                    " - " + s.CompanyName.ToString()).SingleOrDefault();
            }
            if (AgentId != null)
            {
                Input.AgentSenderId = _context.ContactDetails.Where(s => s.SenderId == AgentId).Select
                (
                    s =>
                    s.SenderId.ToString() +
                    " - " + s.CompanyName.ToString()).SingleOrDefault();
            }

            if (CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD1Key] != null)
            {
                MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));
                if (CustomExtensions.NotNullOrEmpty(MSD1.AgentSenderID) || CustomExtensions.NotNullOrEmpty(MSD1.LineSenderID))
                {
                    Input.AgentSenderId = MSD1.AgentSenderID +
                     " - " + MSD1.AgentCompanyName;

                    Input.LineSenderId = MSD1.LineSenderID +
                     " - " + MSD1.LineCompanyName;
                }
            }

            FromSummary = Helpers.ReturnBoolFromQueryString(HttpContext, "FromSummary");

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                InitialisePage();
                return Page();
            }

            MSD1 = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD1Key] != null ? new MSD1(TempData.GetKeep<MSD1>(MSD1Key)) : new MSD1();

            MSD1.AgentSenderID = Input.AgentSenderId.Split('-').FirstOrDefault().TrimEnd();
            MSD1.LineSenderID = Input.LineSenderId.Split('-').FirstOrDefault().TrimEnd();

            MSD1.AgentCompanyName = Input.AgentSenderId.Split('-').LastOrDefault().TrimStart();
            MSD1.LineCompanyName = Input.LineSenderId.Split('-').LastOrDefault().TrimStart();
            TempData.Put(MSD1Key, MSD1);

            return RedirectToPage("./VesselDetails", new { FromSummary = FromSummary.ToString(), IsEdited = FromSummary.ToString() });
        }

        private void InitialisePage()
        {
            LineSenders = AgentSenders = _context.OrgList
            .Where(s => s.IsAgent || s.IsLine)
            .Select
             (
                 s =>
                 s.OrgId.ToString() +
                 " - " + s.OrgName.ToString())
             .ToList();
        }

        public class InputModel
        {
            [Display(Name = "Shipping agent")]
            [Required(ErrorMessage = "Enter a valid shipping agent")]
            public string AgentSenderId { get; set; }

            [Display(Name = "Shipping line or ship operator")]
            [Required(ErrorMessage = "Enter a valid shipping line")]
            public string LineSenderId { get; set; }
        }
    }
}