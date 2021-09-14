using System.ComponentModel.DataAnnotations;

namespace IM.Gateway.Companies.Services.Attributes
{
    public class ZeroAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            string convertedValue = value?.ToString()!;

            bool result = decimal.TryParse(convertedValue, out decimal decimalResult);

            ErrorMessage = "The value must be greater than 0";

            return result && decimalResult != default;

        }
    }
}
