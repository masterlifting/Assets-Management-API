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
    public byte EventTypeId { get; set; }

    public virtual Derivative? Derivative { get; set; }
    public string? DerivativeId { get; set; }

    public virtual Exchange? Exchange { get; set; }
    public byte? ExchangeId { get; set; }

    public virtual Broker Broker { get; set; } = null!;
    public byte BrokerId { get; set; }

    public virtual User User { get; set; } = null!;
    public string UserId { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;
    public string AccountName { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;
    public byte CurrencyId { get; set; }
}