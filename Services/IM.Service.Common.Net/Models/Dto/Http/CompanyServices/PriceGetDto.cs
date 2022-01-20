using System;

namespace IM.Service.Common.Net.Models.Dto.Http.CompanyServices;

public record PriceGetDto
{
    public string Ticker { get; init; } = null!;
    public string Company { get; init; } = null!;
    public DateTime Date { get; init; }
    public string SourceType { get; init; } = null!;
    public decimal Value { get; init; }
    public decimal ValueTrue { get; init; }
    public long? StockVolume { get; init; }
}