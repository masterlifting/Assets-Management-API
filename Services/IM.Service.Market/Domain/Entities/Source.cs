using System.Text.Json.Serialization;
using IM.Service.Common.Net.Models.Entity;
using IM.Service.Market.Domain.Entities.ManyToMany;

namespace IM.Service.Market.Domain.Entities;

public class Source : Catalog
{
    public virtual IEnumerable<CompanySource>? CompanySources { get; init; }

    [JsonIgnore]
    public virtual IEnumerable<Price>? Prices { get; init; }
    [JsonIgnore]
    public virtual IEnumerable<Report>? Reports { get; init; }
    [JsonIgnore]
    public virtual IEnumerable<Coefficient>? Coefficients { get; init; }
    [JsonIgnore]
    public virtual IEnumerable<Dividend>? Dividends { get; init; }
    [JsonIgnore]
    public virtual IEnumerable<Float>? Floats { get; init; }
    [JsonIgnore]
    public virtual IEnumerable<Split>? Splits { get; init; }
}