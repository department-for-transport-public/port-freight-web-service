using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Utilities
{
    public static class Helpers
    {
        public static string GetQueryString(HttpContext httpContext, string param)
        {
            return httpContext.Request.Query[param].ToString() ?? "";
        }

        public static bool ReturnBoolFromQueryString(HttpContext httpContext, string param)
        {
            return bool.TryParse(httpContext.Request.Query[param].ToString(), out bool res)? res : false;
        }
    }
}
