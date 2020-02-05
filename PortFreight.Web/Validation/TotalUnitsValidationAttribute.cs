using PortFreight.Web.Models;
using System.ComponentModel.DataAnnotations;


namespace PortFreight.Web.Validation
{
    public class TotalUnitsValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var msd1ForValidation = validationContext.ObjectInstance as CargoItem; 
            return new ValidationResult("The custom validation has worked on Total Units");
        }
    }
}
