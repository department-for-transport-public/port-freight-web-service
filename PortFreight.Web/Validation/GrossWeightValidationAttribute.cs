using PortFreight.Data;
using PortFreight.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace PortFreight.Web.Validation
{
    public class GrossWeightValidationAttribute : ValidationAttribute
    {
        
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PortFreightContext actualContext = new PortFreightContext();

            var msd1ForValidation = validationContext.ObjectInstance as CargoItem;
            
            return new ValidationResult("The custom validation has worked on Gross Weight");
        }
    }
}
