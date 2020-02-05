using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortFreight.Web.Utilities
{
    public static class CustomExtensions
    {

        public static bool IsNullOrValue(this double? value, double valueToCheck)
        {
            return (value ?? valueToCheck) == valueToCheck;
        }

        public static bool NotNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }
    }
}
