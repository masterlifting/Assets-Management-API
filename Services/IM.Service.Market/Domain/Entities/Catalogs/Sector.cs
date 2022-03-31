using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Market.Domain.Entities.Catalogs;

public class Sector : Catalog
{
    public virtual IEnumerable<Industry>? Industries { get; set; }
}