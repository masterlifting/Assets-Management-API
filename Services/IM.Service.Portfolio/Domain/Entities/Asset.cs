using System.Collections.Generic;
using System.Text.Json.Serialization;
using IM.Service.Portfolio.Domain.Entities.Catalogs;

namespace IM.Service.Portfolio.Domain.Entities;

public class Asset : Shared.Models.Entity.Asset<Asset, AssetType, Country>
{
    [JsonIgnore]
    public virtual IEnumerable<Derivative>? Derivatives { get; set; }
}