using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Domain.Entities.ManyToMany;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class Exchange : Catalog
{
    public virtual IEnumerable<Deal>? Deals { get; set; }
    public virtual IEnumerable<Event>? Events { get; set; }

    public virtual IEnumerable<BrokerExchange>? BrokerExchanges { get; set; }
}