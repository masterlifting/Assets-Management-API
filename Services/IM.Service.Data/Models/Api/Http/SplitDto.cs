namespace IM.Service.Data.Models.Api.Http;

public record SplitGetDto
{
    public string Company { get; init; } = null!;
    public DateOnly Date { get; init; }
    public string SourceType { get; init; } = null!;
    public int Value { get; init; }
}
public class SplitPostDto : SplitPutDto, ICompanyDateIdentity
{
    public string CompanyId { get; init; } = null!;
    public DateOnly Date { get; set; }
}
public class SplitPutDto : StockSplitBody
{
}