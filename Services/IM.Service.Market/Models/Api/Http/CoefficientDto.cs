namespace IM.Service.Market.Models.Api.Http;

public record CoefficientGetDto
{
    public string Company { get; init; } = null!;
    public string Source { get; init; } = null!;
    public int Year { get; init; }
    public byte Quarter { get; init; }

    public decimal? Pe { get; init; }
    public decimal? Pb { get; init; }
    public decimal? DebtLoad { get; init; }
    public decimal? Profitability { get; init; }
    public decimal? Roa { get; init; }
    public decimal? Roe { get; init; }
    public decimal? Eps { get; init; }
}
public record CoefficientPostDto : CoefficientPutDto
{
    public int Year { get; init; }
    public byte Quarter { get; init; }
}
public record CoefficientPutDto
{
    public decimal? Pe { get; init; }
    public decimal? Pb { get; init; }
    public decimal? DebtLoad { get; init; }
    public decimal? Profitability { get; init; }
    public decimal? Roa { get; init; }
    public decimal? Roe { get; init; }
    public decimal? Eps { get; init; }
}