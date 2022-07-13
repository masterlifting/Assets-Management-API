using System;

namespace IM.Service.Portfolio.Models.Api.Http;

public record DealGetDto
{
    public string DerivativeCode { get; init; } = null!;
    public string DerivativeId { get; init; } = null!;
    public string Operation { get; init; } = null!;
    public decimal Value { get; init; }
    public decimal Cost { get; init; }
    public string Currency { get; init; } = null!;

    public string Account { get; init; } = null!;
    public string Provider { get; init; } = null!;
    public string? Exchange { get; init; }

    public DateTime UpdateTime { get; init; }
    public string? Info { get; init; }
}
public record DealPostDto : DealPutDto
{

}
public record DealPutDto
{
    public string AccountUserId { get; init; } = null!;
    public byte AccountBrokerId { get; init; }
    public string AccountName { get; init; } = null!;

    public string DerivativeId { get; init; } = null!;
    public string DerivativeCode { get; init; } = null!;
    public byte ExchangeId { get; init; }
    public byte OperationId { get; init; }
    public byte CurrencyId { get; init; }

    public decimal Cost { get; init; }
    public decimal Value { get; init; }
    public string? Info { get; init; }
}