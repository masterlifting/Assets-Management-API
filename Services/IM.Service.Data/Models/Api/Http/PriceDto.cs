namespace IM.Service.Data.Models.Api.Http;

public record PriceGetDto
{
    public string Ticker { get; init; } = null!;
    public string Company { get; init; } = null!;
    public DateOnly Date { get; init; }
    public string SourceType { get; init; } = null!;
    public decimal Value { get; init; }
    public decimal ValueTrue { get; init; }
    public long? StockVolume { get; init; }
}
public class PricePostDto : PricePutDto, ICompanyDateIdentity
{
    public string CompanyId { get; init; } = null!;
    public DateOnly Date { get; set; }
}
public class PricePutDto : PriceBody, ICompanyDateIdentity
{
    public string CompanyId { get; init; } = null!;
    public DateOnly Date { get; set; }
}