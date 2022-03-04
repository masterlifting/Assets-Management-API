using System.ComponentModel.DataAnnotations;

namespace IM.Service.Data.Domain.Entities.ManyToMany;

public class CompanySource
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;

    public virtual Source Source { get; init; } = null!;
    public byte SourceId { get; init; }


    [StringLength(300, MinimumLength = 1)]
    public string? Value { get; set; }
}