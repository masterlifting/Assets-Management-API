using System.Collections.Generic;

namespace IM.Service.Portfolio.DataAccess.Entities;

public class Derivative
{
    public string Id { get; init; } = null!;
    public string Code { get; init; } = null!;

    public virtual UnderlyingAsset UnderlyingAsset { get; init; } = null!;
    public string UnderlyingAssetId { get; init; } = null!;

    public virtual IEnumerable<Deal>? Deals { get; set; }
    public virtual IEnumerable<Event>? Events { get; set; }
}