using IM.Service.Shared.Models.Entity;

using System.Text.Json.Serialization;

namespace IM.Service.Market.Domain.Entities.Catalogs;

public class Currency : Catalog
{
    [JsonIgnore]
    public virtual IEnumerable<Price>? Prices { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Report>? Reports { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Dividend>? Dividends { get; set; }
}