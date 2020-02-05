using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PortFreight.Web.Pages
{
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult OnGet()
        {
            switch (HttpContext.Response.StatusCode)
            {
                case 404:
                    return RedirectToPage("./Error404");                     
                case 500:
                    return RedirectToPage("./Error500");
                case 503:
                    return RedirectToPage("/Error503");
                default:
                    return RedirectToPage("./Error404");                   
            }
        }
    }
}
