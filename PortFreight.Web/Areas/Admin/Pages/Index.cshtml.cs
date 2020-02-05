using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Web.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace PortFreight.Web.Areas.Admin.Pages
{
    public class IndexModel : PageModel
    {

        private string FindSubKey { get; set; } = "FindSubmissionInput";
        
        public void OnGet()
        {
            TempData.Remove(FindSubKey);
        }
    }
}