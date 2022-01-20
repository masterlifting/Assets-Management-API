using static IM.Service.Company.Analyzer.Enums;
namespace IM.Service.Company.Analyzer.Models.Calculator;

public record struct Sample
{
    public int Id { get; init; }
    public decimal? Value { get; init; }
    public CompareTypes CompareType { get; init; }
}