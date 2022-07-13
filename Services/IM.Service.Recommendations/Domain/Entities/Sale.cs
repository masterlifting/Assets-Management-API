using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Recommendations.Domain.Entities;

public class Sale
{
    [Key]
    public int Id { get; set; }

    public virtual Asset Asset { get; set; } = null!;
    public string AssetId { get; set; } = null!;
    public byte AssetTypeId { get; set; }

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "Decimal(18,10)")]
    public decimal ActiveValue { get; set; }

    [Column(TypeName = "Decimal(18,2)")]
    public decimal ProfitPlan { get; set; }

    [Column(TypeName = "Decimal(18,2)")]
    public decimal? ProfitFact { get; set; }

    [Column(TypeName = "Decimal(18,5)")]
    public decimal CostPlan { get; set; }

    [Column(TypeName = "Decimal(18,5)")]
    public decimal? CostFact { get; set; }

    public bool IsReady { get; init; }
}