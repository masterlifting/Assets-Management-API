using IM.Service.Common.Net.Attributes;

namespace IM.Service.MarketData.Models.Api.Http;

public record FloatGetDto
{
    public string Company { get; init; } = null!;
    public string Source { get; init; } = null!;
    public DateOnly Date { get; init; }
    public long Value { get; init; }
    public long? ValueFree { get; set; }
}
public class FloatPostDto : FloatPutDto
{
    public string CompanyId { get; init; } = null!;
    public byte SourceId { get; init; }
    public DateOnly Date { get; set; }
}
public class FloatPutDto
{
    [NotZero(nameof(Value))]
    public long Value { get; set; }
    public long? ValueFree { get; set; }
}