using System;
using System.ComponentModel.DataAnnotations;
using IM.Service.Portfolio.DataAccess.Entities.Catalogs;

namespace IM.Service.Portfolio.DataAccess.Entities;

public class Deal
{
    [Key]
    public long Id { get; init; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
    public decimal Cost { get; set; }
    public decimal Value { get; set; }
    public string? Info { get; set; }


    public virtual Derivative Derivative { get; set; } = null!;
    public string DerivativeId { get; init; } = string.Empty;

    public virtual Exchange Exchange { get; set; } = null!;
    public byte ExchangeId { get; init; }

    public virtual Broker Broker { get; set; } = null!;
    public byte  BrokerId { get; init; }

    public virtual User User { get; set; } = null!;
    public string UserId { get; init; } = null!;

    public virtual Account Account { get; set; } = null!;
    public string AccountName { get; init; } = null!;

    public virtual Operation Operation { get; set; } = null!;
    public byte OperationId { get; init; }

    public virtual Currency Currency { get; set; } = null!;
    public byte CurrencyId { get; init; }
}