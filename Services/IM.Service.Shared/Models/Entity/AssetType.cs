using IM.Service.Shared.Models.Entity.Interfaces;

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IM.Service.Shared.Models.Entity;

public class AssetType<TAsset> : Catalog, IAssetType<TAsset> where TAsset: class, IAsset
{
    [JsonIgnore]
    public virtual IEnumerable<TAsset>? Assets { get; set; }
}