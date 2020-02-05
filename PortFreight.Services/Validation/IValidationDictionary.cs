using System;
using System.Collections.Generic;
using System.Text;

namespace PortFreight.Services.Validation
{
    public interface IValidationDictionary
    {
        void AddError(string key, string errorMessage);
    }
}
