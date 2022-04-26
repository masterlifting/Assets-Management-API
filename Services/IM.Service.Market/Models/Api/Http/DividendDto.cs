using IM.Service.Common.Net.Attributes;

namespace IM.Service.Market.Models.Api.Http;

public record DividendGetDto
{
    public string Company { get; init; } = null!;
    public string Source { get; init; } = null!;
    public DateOnly Date { get; init; }
    public string Currency { get; init; } = null!;
    public decimal Value { get; init; }
}
public record DividendPostDto : DividendPutDto
{
    public DateOnly Date { get; init; }
}
public record DividendPutDto
{
    [MoreZero(nameof(CurrencyId))]
    public byte CurrencyId { get; init; }
    [MoreZero(nameof(Value))]
    public decimal Value { get; init; }
}