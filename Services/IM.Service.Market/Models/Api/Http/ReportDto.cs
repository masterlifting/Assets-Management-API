using System.ComponentModel.DataAnnotations;
using IM.Service.Common.Net.Attributes;

namespace IM.Service.Market.Models.Api.Http;

public record ReportGetDto
{
    public string Company { get; init; } = null!;
    public string Source { get; init; } = null!;
    public int Year { get; init; }
    public byte Quarter { get; init; }

    public int Multiplier { get; init; }
    public string Currency { get; init; } = null!;


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
    public int Year { get; init; }
    public byte Quarter { get; init; }
}
public record ReportPutDto
{
    [MoreZero(nameof(CurrencyId))]
    public byte CurrencyId { get; init; }

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