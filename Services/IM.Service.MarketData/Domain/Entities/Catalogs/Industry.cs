using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.MarketData.Domain.Entities.Catalogs;

public class Industry : Catalog
{
    public virtual IEnumerable<Company>? Companies { get; set; }

    public virtual Sector Sector { get; set; } = null!;
    public byte SectorId { get; set; }
}