using System.ComponentModel.DataAnnotations;

namespace IM.Service.Company.Data.DataAccess.Entities.ManyToMany;

public class CompanySource
{
    public int Id { get; set; }
    
    public virtual Company Company { get; set; } = null!;
    public string CompanyId { get; set; } = null!;

    public virtual Sources Source { get; set; } = null!;
    public byte SourceId { get; set; }

    [StringLength(300, MinimumLength = 2)]
    public string? Value { get; set; }
}