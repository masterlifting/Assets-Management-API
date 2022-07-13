using System.Collections.Generic;
using System.Text.Json.Serialization;
using IM.Service.Shared.Models.Entity.Interfaces;

namespace IM.Service.Shared.Models.Entity;

public class Country<TAsset> : Catalog, ICountry<TAsset> where TAsset : class, IAsset 
{
    [JsonIgnore]
    public virtual IEnumerable<TAsset>? Assets { get; set; }
}