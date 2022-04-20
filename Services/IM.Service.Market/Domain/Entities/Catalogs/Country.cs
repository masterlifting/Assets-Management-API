using System.Text.Json.Serialization;

using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Market.Domain.Entities.Catalogs;

public class Country : Catalog
{
    [JsonIgnore]
    public virtual IEnumerable<Company>? Companies { get; set; }
}