using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Utilities
{
    public class BaseMsd1PageModel : PageModel
    {
        protected readonly string MSD1Key = "MSD1Key";
        protected readonly string MSD1Success = "MSD1Success";
        protected readonly string MSD3Key = "MSD3Key";
        protected readonly string MSD23Key = "MSD23Key";
        protected readonly string RespondentVMKey = "RespondentVMKey";
        private DynamicViewData _viewBag;

        public dynamic ViewBag
        {
            get
            {
                if (_viewBag == null)
                {
                    _viewBag = new DynamicViewData(() => ViewData);
                }
                return _viewBag;
            }
        }
    }
}
