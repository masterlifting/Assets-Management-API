using System;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Shared.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class LowerAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        string? oldValue = null;

        if (value is string)
        {
            oldValue = value.ToString();
            value = oldValue?.ToLowerInvariant();
        }
        else
            ErrorMessage = $"The '{value}' must be string!";

        return oldValue is not null
               && value is string newValue
               && oldValue.Equals(newValue, StringComparison.InvariantCulture);
    }
}