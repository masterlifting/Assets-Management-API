using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IM.Service.Company.Data.DataAccess.Entities.ManyToMany;

namespace IM.Service.Company.Data.DataAccess.Entities;

public class Company : Common.Net.Models.Entity.Company
{
    public byte IndustryId { get; set; }
    public virtual Industry Industry { get; set; } = null!;
    
    [StringLength(300)]
    public string? Description { get; set; }

    public virtual IEnumerable<CompanySource>? CompanySources { get; set; }

    public virtual IEnumerable<Price>? Prices { get; init; }
    public virtual IEnumerable<Report>? Reports { get; init; }
    public virtual IEnumerable<StockSplit>? StockSplits { get; init; }
    public virtual IEnumerable<StockVolume>? StockVolumes { get; init; }
}