using System.ComponentModel.DataAnnotations;

namespace PortFreight.ViewModel.Validators
{
    public class SpaceNotAllowedAttribute : ValidationAttribute
    {        
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;
            else
            {
                return value.ToString().Contains(" ")
                    ? new ValidationResult(string.Format("Spaces are not allowed in {0} field", validationContext.DisplayName))
                    : ValidationResult.Success;
            }
        }
    }
}
