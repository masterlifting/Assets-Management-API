using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IM.Service.Shared.Attributes;
using IM.Service.Market.Domain.Entities.Catalogs;
using IM.Service.Market.Domain.Entities.ManyToMany;

namespace IM.Service.Market.Domain.Entities;

public class Company
{
    [Key, StringLength(10, MinimumLength = 1), Upper]
    public string Id { get; init; } = null!;

    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = null!;

    public virtual Industry Industry { get; set; } = null!;
    public byte IndustryId { get; set; }

    public virtual Country Country { get; set; } = null!;
    public byte CountryId { get; set; }
    
    [StringLength(300)]
    public string? Description { get; set; }

    public virtual Rating? Rating { get; set; }

    public virtual IEnumerable<CompanySource>? Sources { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Price>? Prices { get; init; }
    [JsonIgnore]
    public virtual IEnumerable<Report>? Reports { get; init; }
    [JsonIgnore]
    public virtual IEnumerable<Coefficient>? Coefficients { get; init; }
    [JsonIgnore]
    public virtual IEnumerable<Dividend>? Dividends { get; init; }
    [JsonIgnore]
    public virtual IEnumerable<Split>? Splits { get; init; }
    [JsonIgnore]
    public virtual IEnumerable<Float>? Floats { get; init; }
}