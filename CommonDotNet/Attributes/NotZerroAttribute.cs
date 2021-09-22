using System.ComponentModel.DataAnnotations;

namespace CommonServices.Attributes
{
    public class NotZeroAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var stringValue = value?.ToString();

            var isParse = decimal.TryParse(stringValue, out var result);

            ErrorMessage = "The value must be greater than 0";

            return isParse && result > 0;

        }
    }
}
