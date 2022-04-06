using System.ComponentModel.DataAnnotations;

namespace IM.Service.Common.Net.Attributes;

public class MoreZeroAttribute : ValidationAttribute
{
    private readonly string property;
    public MoreZeroAttribute(string property) => this.property = property;

    public override bool IsValid(object? value)
    {
        var stringValue = value?.ToString();

        var isParse = decimal.TryParse(stringValue, out var result);

        ErrorMessage = $"The '{property}' must be greater than 0";

        return isParse && result > 0;
    }
}