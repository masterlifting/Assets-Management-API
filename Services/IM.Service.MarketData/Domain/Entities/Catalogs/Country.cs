using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.MarketData.Domain.Entities.Catalogs;

public class Country : Catalog
{
    public virtual IEnumerable<Company>? Companies { get; set; }
}