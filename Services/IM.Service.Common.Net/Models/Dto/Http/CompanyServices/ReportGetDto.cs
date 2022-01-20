namespace IM.Service.Common.Net.Models.Dto.Http.CompanyServices;

public record ReportGetDto
{
    public string Ticker { get; init; } = null!;
    public string Company { get; init; } = null!;
    public int Year { get; init; }
    public byte Quarter { get; init; }
    public string SourceType { get; init; } = null!;

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