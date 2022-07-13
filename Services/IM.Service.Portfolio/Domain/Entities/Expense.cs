using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IM.Service.Portfolio.Domain.Entities;

public class Expense
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; init; }

    public virtual Derivative Derivative { get; set; } = null!;
    public string DerivativeId { get; set; } = null!;
    public string DerivativeCode { get; set; } = null!;

    [JsonIgnore]
    public virtual Deal Deal { get; set; } = null!;
    public string DealId { get; set; } = null!;

    [Column(TypeName = "Decimal(18,10)")]
    public decimal Value { get; set; }

    public DateOnly Date { get; set; }
}