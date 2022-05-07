using System;
using System.ComponentModel.DataAnnotations;
using IM.Service.Portfolio.Domain.Entities.Catalogs;

namespace IM.Service.Portfolio.Domain.Entities;

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
    public string? DerivativeCode { get; set; }

    public virtual Exchange? Exchange { get; set; }
    public byte? ExchangeId { get; set; }

    public virtual Account Account { get; set; } = null!;
    public string AccountUserId { get; set; } = null!;
    public byte AccountBrokerId { get; set; }
    public string AccountName { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;
    public byte CurrencyId { get; set; }
}