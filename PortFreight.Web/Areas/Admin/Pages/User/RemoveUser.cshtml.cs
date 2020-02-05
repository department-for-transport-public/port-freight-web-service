using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Services.User;

namespace PortFreight.Web.Areas.Admin.Pages.User
{
    [Authorize(Roles = "Admin")]
    public class RemoveUserModel : PageModel
    {
        [BindProperty]
        public string SearchParam { get; set; }
        public List<PortFreightUser> Users { get; set; } = new List<PortFreightUser>();
        public List<PortFreightUser> DeletedUsers { get; set; } = new List<PortFreightUser>();
        private readonly IUserService _userService;
        private readonly ILogger<RemoveUserModel> _logger;

        public RemoveUserModel(IUserService userService)
        {
            _userService = userService;
        }

        public void OnGet()
        {
            DeletedUsers = _userService.GetDisabledUsers();
            if (DeletedUsers.Count == 0)
            {
                ModelState.AddModelError("NoUsers", "No deleted users found");
            }

        }

        public IActionResult OnPostSearch()
        {
            Users = _userService.GetUsersListByEmailOrSenderID(SearchParam ?? string.Empty);

            if (Users.Count == 0)
            {
                ModelState.AddModelError("NoUsers", "No results found");
            }

            return Page();
        }

        public IActionResult OnPostViewDeleted()
        {
            return Page();
        }

        public IActionResult OnPostReinstate(string ID)
        {
            try
            {
                _userService.ReinstateUser(ID);

                ModelState.AddModelError("Success", "User reinstated");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);

                ModelState.AddModelError("Error", "There has been an error reinstating this user. Please try again");
            }

            DeletedUsers = _userService.GetDisabledUsers();

            return Page();
        }

        public IActionResult OnPostRemoveUser(string ID)
        {
            try
            {
                _userService.DisableUser(ID);

                ModelState.AddModelError("Success", "User removed");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);

                ModelState.AddModelError("Error", "There has been an error removing this user. Please try again");
            }

            DeletedUsers = _userService.GetDisabledUsers();

            return Page();
        }
    }
}