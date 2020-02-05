using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services;

namespace PortFreight.Web.Pages
{
    [BindProperties]
    public class ApiKeyModel : PageModel
    {
        private IApiKeyService _apiKeyService;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly IConfiguration _config;

        public ApiKey ApiKey { get; set; }

        public ApiKeyModel(IApiKeyService apiKeyService, UserManager<PortFreightUser> userManager, IConfiguration config)
        {
            _apiKeyService = apiKeyService;
            _userManager = userManager;
            _config = config;
        }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user != null && !string.IsNullOrEmpty(user.SenderId))
                {
                    ApiKey = _apiKeyService.GetApiKey(user.SenderId);
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("ApiKey", "Failed to retrieve Api Key");
                return Page();
            }
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(HttpContext.User);
                    var isSucceed = false;

                    if (user != null)
                    {
                        DataSource source = DataSource.ASCII;
                        var skipProfileIds = _config["SkipProfile:SenderId"];
                        if (skipProfileIds != null && skipProfileIds.Contains(user.SenderId)) {
                            source = DataSource.GESMES;
                        }

                        isSucceed = _apiKeyService.CreateApiKey(user.SenderId, source);
                        ApiKey = _apiKeyService.GetApiKey(user.SenderId);
                    }

                    if (!isSucceed)
                    {
                        ModelState.AddModelError("ApiKeyError", "Failed to generate a new key");
                    }
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("ApiKeyError", "Failed to generate a new key");
                return Page();
            }
            return Page();
        }
    }
}