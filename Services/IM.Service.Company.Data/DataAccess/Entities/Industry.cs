using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Company.Data.DataAccess.Entities;

public class Industry : CommonEntityType
{
    public virtual IEnumerable<Company>? Companies { get; set; }

    public virtual Sector Sector { get; set; } = null!;
    public byte SectorId { get; set; }
}