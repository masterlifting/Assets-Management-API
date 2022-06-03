using IM.Service.Shared.Attributes;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Recommendations.Domain.Entities;

public class Company
{
    [Key, StringLength(10, MinimumLength = 1), Upper]
    public string Id { get; init; } = null!;

    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = null!;

    public int? RatingPlace { get; set; }
    public decimal? DealValue { get; set; }
    public decimal? DealCost { get; set; }
    public decimal? PriceLast { get; set; }
    public decimal? PriceAvg { get; set; }

    public virtual IEnumerable<Purchase>? Purchases { get; set; }
    public virtual IEnumerable<Sale>? Sales { get; set; }
}