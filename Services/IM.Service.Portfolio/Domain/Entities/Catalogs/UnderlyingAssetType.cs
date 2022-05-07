using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity;
using IM.Service.Portfolio.Domain.Entities;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class UnderlyingAssetType : Catalog
{
    public virtual IEnumerable<UnderlyingAsset>? UnderlyingAssets { get; set; }
}