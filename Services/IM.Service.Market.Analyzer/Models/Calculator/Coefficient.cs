namespace IM.Service.Market.Analyzer.Models.Calculator;

public record Coefficient
{
    public decimal Pe { get; init; }
    public decimal Pb { get; init; }
    public decimal DebtLoad { get; init; }
    public decimal Profitability { get; init; }
    public decimal Roa { get; init; }
    public decimal Roe { get; init; }
    public decimal Eps { get; init; }
}