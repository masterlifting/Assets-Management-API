using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IM.Service.Market.Domain.Entities.Interfaces;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Domain.Entities.ManyToMany;

public class CompanySource : IDataIdentity
{
    [JsonIgnore]
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; set; } = null!;
    
    [JsonIgnore]
    public virtual Source Source { get; init; } = null!;
    public byte SourceId { get; set; } = (byte)Sources.Manual;


    [StringLength(300, MinimumLength = 1)]
    public string? Value { get; set; }
}