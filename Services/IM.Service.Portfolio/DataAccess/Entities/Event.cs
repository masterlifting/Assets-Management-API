using System;
using System.ComponentModel.DataAnnotations;
using IM.Service.Portfolio.DataAccess.Entities.Catalogs;

namespace IM.Service.Portfolio.DataAccess.Entities;

public class Event
{
    [Key]
    public long Id { get; init; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
    public decimal Cost { get; set; }
    public string? Info { get; set; }


    public virtual EventType EventType { get; set; } = null!;
    public byte EventTypeId { get; init; }

    public virtual Derivative? Derivative { get; set; }
    public string? DerivativeId { get; init; }

    public virtual Exchange? Exchange { get; set; }
    public byte? ExchangeId { get; init; }

    public virtual Broker Broker { get; set; } = null!;
    public byte BrokerId { get; init; }

    public virtual User User { get; set; } = null!;
    public string UserId { get; init; } = null!;

    public virtual Account Account { get; set; } = null!;
    public string AccountName { get; init; } = null!;

    public virtual Currency Currency { get; set; } = null!;
    public byte CurrencyId { get; init; }
}