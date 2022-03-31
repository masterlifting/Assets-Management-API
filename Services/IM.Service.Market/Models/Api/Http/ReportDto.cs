using System.ComponentModel.DataAnnotations;

namespace IM.Service.Market.Models.Api.Http;

public record ReportGetDto
{
    public string Company { get; init; } = null!;
    public string Source { get; init; } = null!;
    public int Year { get; init; }
    public byte Quarter { get; init; }

    public int Multiplier { get; init; }

    public decimal? Revenue { get; init; }
    public decimal? ProfitNet { get; init; }
    public decimal? ProfitGross { get; init; }
    public decimal? CashFlow { get; init; }
    public decimal? Asset { get; init; }
    public decimal? Turnover { get; init; }
    public decimal? ShareCapital { get; init; }
    public decimal? Obligation { get; init; }
    public decimal? LongTermDebt { get; init; }
}
public record ReportPostDto : ReportPutDto
{
    public string CompanyId { get; init; } = null!;
    public byte SourceId { get; init; }
    public int Year { get; init; }
    public byte Quarter { get; init; }
}
public record ReportPutDto
{
    [Range(1, int.MaxValue)]
    public int Multiplier { get; init; }

    public decimal? Revenue { get; init; }
    public decimal? ProfitNet { get; init; }
    public decimal? ProfitGross { get; init; }
    public decimal? CashFlow { get; init; }
    public decimal? Asset { get; init; }
    public decimal? Turnover { get; init; }
    public decimal? ShareCapital { get; init; }
    public decimal? Obligation { get; init; }
    public decimal? LongTermDebt { get; init; }
}