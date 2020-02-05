using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortFreight.Services.Validation
{
    public class ModelStateAdapter : IValidationDictionary
    {
        private ModelStateDictionary _modelState ;

        public ModelStateAdapter()
        {
           _modelState = new ModelStateDictionary();
        }

        public void AddError(string key, string errorMessage)
        {
            _modelState.AddModelError(key, errorMessage);
        }
    }
}
