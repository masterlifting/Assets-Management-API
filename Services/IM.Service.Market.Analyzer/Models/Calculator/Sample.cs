using static IM.Service.Market.Analyzer.Enums;
namespace IM.Service.Market.Analyzer.Models.Calculator;

public record struct Sample
{
    public int Id { get; init; }
    public decimal? Value { get; init; }
    public CompareTypes CompareType { get; init; }
}