using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity;
using IM.Service.Portfolio.Domain.Entities;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class Currency : Catalog
{
    public virtual IEnumerable<Deal>? Deals { get; set; }
    public virtual IEnumerable<Event>? Events { get; set; }
}