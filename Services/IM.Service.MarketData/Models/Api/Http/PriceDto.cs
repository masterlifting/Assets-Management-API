using IM.Service.Common.Net.Attributes;

namespace IM.Service.MarketData.Models.Api.Http;

public record PriceGetDto
{
    public string Company { get; init; } = null!;
    public string Source { get; init; } = null!;
    public DateOnly Date { get; init; }
    public decimal Value { get; init; }
    public decimal ValueTrue { get; init; }
}
public class PricePostDto : PricePutDto
{
    public string CompanyId { get; init; } = null!;
    public byte SourceId { get; init; }
    public DateOnly Date { get; set; }
}
public class PricePutDto
{
    [NotZero(nameof(Value))]
    public decimal Value { get; set; }
}