using IM.Service.Common.Net.Attributes;

namespace IM.Service.Market.Models.Api.Http;

public record PriceGetDto
{
    public string Company { get; init; } = null!;
    public string Source { get; init; } = null!;
    public DateOnly Date { get; init; }
    public decimal Value { get; init; }
    public decimal ValueTrue { get; init; }
}
public record PricePostDto : PricePutDto
{
    public string CompanyId { get; init; } = null!;
    public byte SourceId { get; init; }
    public DateOnly Date { get; init; }
}
public record PricePutDto
{
    [MoreZero(nameof(Value))]
    public decimal Value { get; init; }
}