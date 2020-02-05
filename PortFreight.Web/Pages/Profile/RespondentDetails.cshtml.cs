using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Pages.Profile
{
    public class RespondentDetailsModel : BaseMsd1PageModel
    {
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly PortFreightContext _context;
        private readonly IHelperService _helperService;

        public string SuccessMessage { get; set; }

        [BindProperty]
        [SenderTypeValidation]
        public SenderType SenderType { get; set; }
        public SenderIdPort SenderIdPort { get; set; }

        public RespondentDetailsModel(UserManager<PortFreightUser> userManager, PortFreightContext context, IHelperService helperService)
        {
            _userManager = userManager;
            _context = context;
            _helperService = helperService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return RedirectToPage("/Account/Logout", new { area = "Identity" });
            }

            SenderType = _context.SenderType
               .Where(x => x.SenderId == user.SenderId)
               .FirstOrDefault() ?? new SenderType() { SenderId = user.SenderId };

            return Page();
        }

        private SenderType FindSavedPort(PortFreightUser user)
        {
            return _context.SenderType.Include(p => p.SenderIdPort)
                .Where(p => p.SenderId == user.SenderId)
                .Include(p => p.SenderIdPort)
                .SingleOrDefault();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _context.SenderType.AnyAsync(x => x.SenderId == SenderType.SenderId) ? _context.Update(SenderType) : _context.Add(SenderType);
            await _context.SaveChangesAsync();
            SuccessMessage = "Respondent details successfully saved";

            return Page(); //RedirectToPage("/Dashboard");
        }

    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SenderTypeValidationAttribute : ValidationAttribute, IClientModelValidator
    {
        public SenderTypeValidationAttribute()
        {

        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-SenderType.IsAgent", GetErrorMessage());
        }

        private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }
            attributes.Add(key, value);
            return true;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            SenderType senderType = (SenderType)validationContext.ObjectInstance;
            var containerType = validationContext.ObjectInstance.GetType();

            if (!senderType.IsAgent && !senderType.IsLine && !senderType.IsPort)
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }
        private string GetErrorMessage()
        {
            return "Select at least one Sender Type";
        }
    }
}