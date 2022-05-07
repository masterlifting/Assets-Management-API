using System;
using System.ComponentModel.DataAnnotations;
using IM.Service.Portfolio.Domain.Entities.Catalogs;

namespace IM.Service.Portfolio.Domain.Entities;

public class Deal
{
    [Key]
    public long Id { get; init; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
    public decimal Cost { get; set; }
    public decimal Value { get; set; }
    public string? Info { get; set; }


    public virtual Derivative Derivative { get; set; } = null!;
    public string DerivativeId { get; set; } = null!;
    public string DerivativeCode { get; set; } = null!;

    public virtual Exchange Exchange { get; set; } = null!;
    public byte ExchangeId { get; set; }

    public virtual Account Account { get; set; } = null!;
    public string AccountUserId { get; set; } = null!;
    public byte AccountBrokerId { get; set; }
    public string AccountName { get; set; } = null!;

    public virtual Operation Operation { get; set; } = null!;
    public byte OperationId { get; set; }

    public virtual Currency Currency { get; set; } = null!;
    public byte CurrencyId { get; set; }
}