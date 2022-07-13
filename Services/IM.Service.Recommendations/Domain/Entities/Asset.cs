using IM.Service.Recommendations.Domain.Entities.Catalogs;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Recommendations.Domain.Entities;

public class Asset : Shared.Models.Entity.Asset<Asset, AssetType, Country>
{
    public int? RatingPlace { get; set; }
    [Column(TypeName = "Decimal(18,10)")]
    public decimal? DealSumValue { get; set; }
    [Column(TypeName = "Decimal(18,5)")]
    public decimal? DealSumCost { get; set; }
    [Column(TypeName = "Decimal(18,5)")]
    public decimal? DealCostLast { get; set; }
    [Column(TypeName = "Decimal(18,5)")]
    public decimal? CostFact { get; set; }
    [Column(TypeName = "Decimal(18,5)")]
    public decimal? CostAvg { get; set; }

    public virtual IEnumerable<Purchase>? Purchases { get; set; }
    public virtual IEnumerable<Sale>? Sales { get; set; }
}