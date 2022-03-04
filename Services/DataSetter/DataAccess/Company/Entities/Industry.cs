using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataSetter.DataAccess.Company.Entities;

public class Industry : Catalog
{
    public virtual IEnumerable<Company>? Companies { get; set; }

    public virtual Sector Sector { get; set; } = null!;
    [Range(1, byte.MaxValue)]
    public byte SectorId { get; set; }
}