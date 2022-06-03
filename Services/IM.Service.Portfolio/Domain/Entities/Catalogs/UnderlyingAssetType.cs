using IM.Service.Shared.Models.Entity;

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class UnderlyingAssetType : Catalog
{
    [JsonIgnore]
    public virtual IEnumerable<UnderlyingAsset>? UnderlyingAssets { get; set; }
}