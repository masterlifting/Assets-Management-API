using System.ComponentModel.DataAnnotations;

namespace IM.Service.Company.Data.DataAccess.Entities.ManyToMany;

public class CompanySource
{
    public virtual Company Company { get; set; } = null!;
    public string CompanyId { get; init; } = null!;

    public virtual Sources Source { get; set; } = null!;
    public byte SourceId { get; init; }

    [StringLength(300, MinimumLength = 1)]
    public string? Value { get; set; }
}