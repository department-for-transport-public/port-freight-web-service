using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PortFreight.Data;
using PortFreight.Web.Utilities;
using Microsoft.Extensions.Logging;
using PortFreight.Services.Interface;
using PortFreight.Services.Common;

namespace PortFreight.Web.Areas.Admin.Pages.WorldFleet
{
    public class ShipListUploadModel : PageModel
    {
        [BindProperty]
        public string UploadMethodRetunMessage { get; set; }

        private const string shipLoadRequestedKey = "ShipListUploadRequested";
        private const string shipUpoadFileNameKey = "UploadedFileName";
        private readonly PortFreightContext _context;
        private readonly UserManager<PortFreightUser> _userManager;
        private IShipListBulkUpload _csvImport;
        private readonly ILogger<ShipListUploadModel> _logger;

        public ShipListUploadModel(IShipListBulkUpload csvImport,
               UserManager<PortFreightUser> userManager,
               PortFreightContext context,
               ILogger<ShipListUploadModel> logger)
        {
            _csvImport = csvImport;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPostUploadFile(IFormFile UploadedFile)
        {                     
            if (TempData.Peek(shipLoadRequestedKey) == null)
            {
                TempData.Put(shipLoadRequestedKey, "true");
            }
            if (TempData.Peek(shipUpoadFileNameKey) != null)
            {
                TempData.Remove(shipUpoadFileNameKey);
                TempData.Put(shipUpoadFileNameKey, UploadedFile.FileName);
            }
            var returnValue = _csvImport.BulkUploadShipList(UploadedFile, HttpContext.User.Identity.Name);

            if (returnValue.SuccessFaliure == Enums.MethodResultOutcome.Failure)
            {
                ModelState.AddModelError("UploadedFile", returnValue.Message);
            }

            UploadMethodRetunMessage = returnValue.Message;
             return Page();           
        }
    }
}
  