using IM.Service.Portfolio.Domain.Entities.Catalogs;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IM.Service.Portfolio.Enums;
using static IM.Service.Shared.Enums;

namespace IM.Service.Portfolio.Domain.Entities;

public class Deal
{
    [Key, StringLength(40)] 
    public string Id { get; init; } = null!;

    public DateOnly Date { get; set; }

    [Required, StringLength(500)]
    public string Info { get; set; } = null!;

    [Column(TypeName = "Decimal(18,10)")]
    public decimal Cost { get; set; }

    public virtual Income Income { get; init; } = null!;
    public virtual Expense Expense { get; init; } = null!;

    public virtual Account Account { get; set; } = null!;
    public int AccountId { get; set; }

    public virtual User User { get; set; } = null!;
    public string UserId { get; set; } = null!;

    public virtual Provider Provider { get; set; } = null!;
    public int ProviderId { get; set; } = (int)Providers.Default;

    public virtual Exchange Exchange { get; set; } = null!;
    public byte ExchangeId { get; set; } = (byte)Exchanges.Default;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
}