namespace IM.Service.Data.Models.Api.Http;

public record FloatGetDto
{
    public string Company { get; init; } = null!;
    public DateOnly Date { get; init; }
    public string SourceType { get; init; } = null!;
    public long Value { get; init; }
}
public class FloatPostDto : FloatPutDto, ICompanyDateIdentity
{
    public string CompanyId { get; init; } = null!;
    public DateOnly Date { get; set; }
}
public class FloatPutDto : StockVolumeBody
{
}