using IM.Service.Common.Net.Attributes;

namespace IM.Service.Market.Models.Api.Http;

public record FloatGetDto
{
    public string Company { get; init; } = null!;
    public string Source { get; init; } = null!;
    public DateOnly Date { get; init; }
    public long Value { get; init; }
    public long? ValueFree { get; init; }
}
public record FloatPostDto : FloatPutDto
{
    public string CompanyId { get; init; } = null!;
    public byte SourceId { get; init; }
    public DateOnly Date { get; init; }
}
public record FloatPutDto
{
    [MoreZero(nameof(Value))]
    public long Value { get; init; }
    public long? ValueFree { get; init; }
}