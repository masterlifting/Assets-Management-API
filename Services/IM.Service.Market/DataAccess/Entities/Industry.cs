using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Market.DataAccess.Entities;

public class Industry : Catalog
{
    public virtual IEnumerable<Company>? Companies { get; set; }

    public virtual Sector Sector { get; set; } = null!;
    public byte SectorId { get; set; }
}