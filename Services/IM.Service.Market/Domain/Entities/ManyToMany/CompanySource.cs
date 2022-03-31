using System.ComponentModel.DataAnnotations;
using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Domain.Entities.ManyToMany;

public class CompanySource : IDataIdentity
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; set; } = null!;

    public virtual Source Source { get; init; } = null!;
    public byte SourceId { get; set; }


    [StringLength(300, MinimumLength = 1)]
    public string? Value { get; set; }
}