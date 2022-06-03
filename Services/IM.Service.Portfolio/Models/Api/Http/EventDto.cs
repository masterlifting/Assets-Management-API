using System;

namespace IM.Service.Portfolio.Models.Api.Http;

public record EventGetDto
{
    public string Event { get; init; } = null!;
    public string Account { get; init; } = null!;
    public string Broker { get; init; } = null!;

    public decimal Cost { get; init; }
    public string Currency { get; init; } = null!;

    public string? DerivativeCode { get; init; }
    public string? DerivativeId { get; init; }
    public string? Exchange { get; init; }

    public DateTime UpdateTime { get; init; }
    public string? Info { get; init; }
}
public record EventPostDto : EventPutDto
{

}
public record EventPutDto
{
    public byte EventTypeId { get; set; }

    public string AccountUserId { get; init; } = null!;
    public byte AccountBrokerId { get; init; }
    public string AccountName { get; init; } = null!;

    public byte CurrencyId { get; init; }
    public decimal Cost { get; init; }
    public string? Info { get; init; }

    public string? DerivativeId { get; init; }
    public string? DerivativeCode { get; init; }
    public byte? ExchangeId { get; init; }
}