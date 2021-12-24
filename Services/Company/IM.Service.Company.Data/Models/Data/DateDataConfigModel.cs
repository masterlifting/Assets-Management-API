using System;

namespace IM.Service.Company.Data.Models.Data;

public record DateDataConfigModel
{
    public string CompanyId { get; init; } = null!;
    public DateOnly Date { get; init; }
    public string? SourceValue { get; init; }
}