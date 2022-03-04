using System;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Common.Net.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class UpperAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        string? oldValue = null;
            
        if (value is string)
        {
            oldValue = value.ToString();
            value = oldValue?.ToUpperInvariant();
        }
        else
            ErrorMessage = $"The '{value}' must be string!";

        return oldValue is not null 
               && value is string newValue 
               && oldValue.Equals(newValue, StringComparison.InvariantCulture);
    }
}