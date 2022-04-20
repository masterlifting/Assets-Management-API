using IM.Service.Common.Net.Models.Entity;

using System.Text.Json.Serialization;

namespace IM.Service.Market.Domain.Entities.Catalogs;

public class Status : Catalog
{
    [JsonIgnore]
    public virtual IEnumerable<Report>? Reports { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Price>? Prices { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Coefficient>? Coefficients { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Dividend>? Dividends { get; set; }
}