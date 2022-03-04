namespace IM.Service.Data.Models.Services;

public record DataConfigModel
{
    public string CompanyId { get; init; } = null!;
    public string? SourceValue { get; init; }
    public bool IsCurrent { get; init; }
}