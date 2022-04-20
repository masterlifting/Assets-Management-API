using IM.Service.Common.Net.Attributes;

namespace IM.Service.Market.Models.Api.Http;

public record SplitGetDto
{
    public string Company { get; init; } = null!;
    public string Source { get; init; } = null!;
    public DateOnly Date { get; init; }
    public int Value { get; init; }
}
public record SplitPostDto : SplitPutDto
{
    public DateOnly Date { get; init; }
}
public record SplitPutDto
{
    [MoreZero(nameof(Value))]
    public int Value { get; init; }
}