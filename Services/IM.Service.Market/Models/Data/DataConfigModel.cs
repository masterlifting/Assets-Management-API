namespace IM.Service.Market.Models.Data;

public record DataConfigModel
{
    public string CompanyId { get; init; } = null!;
    public string? SourceValue { get; init; }
    public bool IsCurrent { get; init; }
}