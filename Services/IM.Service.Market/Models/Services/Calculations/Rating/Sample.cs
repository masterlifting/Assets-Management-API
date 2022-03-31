namespace IM.Service.Market.Models.Services.Calculations.Rating;

public record struct Sample
{
    public int Id { get; init; }
    public decimal? Value { get; init; }
    public Enums.CompareTypes CompareType { get; init; }
}