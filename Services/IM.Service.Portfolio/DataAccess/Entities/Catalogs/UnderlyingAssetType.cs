using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Portfolio.DataAccess.Entities.Catalogs;

public class UnderlyingAssetType : CommonEntityType
{
    public virtual IEnumerable<UnderlyingAsset>? UnderlyingAssets { get; set; }
}