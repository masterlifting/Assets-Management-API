using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using IM.Service.Shared.Attributes;
using static IM.Service.Shared.Enums;

namespace IM.Service.Portfolio.Domain.Entities;

public class Derivative
{
    [StringLength(20, MinimumLength = 1), Upper]
    public string Id { get; init; } = null!;
    [StringLength(50, MinimumLength = 1), Upper]
    public string Code { get; init; } = null!;

    public virtual Asset Asset { get; set; } = null!;
    public string AssetId { get; set; } = null!;
    public byte AssetTypeId { get; set; } = (byte) AssetTypes.Default;

    [Column(TypeName = "Decimal(18,10)")]
    public decimal Balance { get; set; }

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public virtual IEnumerable<Income>? Incomes { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Expense>? Expenses { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Event>? Events { get; set; }
}