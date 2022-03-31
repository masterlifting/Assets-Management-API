using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Market.Domain.Entities.Catalogs;

public class Currency : Catalog
{
    public virtual IEnumerable<Price>? Prices { get; set; }
    public virtual IEnumerable<Report>? Reports { get; set; }
    public virtual IEnumerable<Dividend>? Dividends { get; set; }
}