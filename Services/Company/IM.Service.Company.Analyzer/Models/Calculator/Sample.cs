using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Models.Calculator;

public record struct Sample
{
    public string CompanyId { get; init; } = null!;
    public decimal Value { get; init; }
    public CompareTypes CompareTypes { get; init; }
}