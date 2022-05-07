using IM.Service.Common.Net.Models.Entity;
using IM.Service.Portfolio.Domain.Entities.ManyToMany;

using System.Collections.Generic;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class Broker : Catalog
{
    public virtual IEnumerable<Account>? Accounts { get; set; }
    public virtual IEnumerable<BrokerUser>? BrokerUsers { get; set; }
    public virtual IEnumerable<BrokerExchange>? BrokerExchanges { get; set; }
}