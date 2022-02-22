using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;
using IM.Service.Portfolio.DataAccess.Entities.ManyToMany;

namespace IM.Service.Portfolio.DataAccess.Entities.Catalogs;

public class Broker : CommonEntityType
{
    public virtual IEnumerable<Account>? Accounts { get; set; }
             
    public virtual IEnumerable<Deal>? Deals { get; set; }
    public virtual IEnumerable<Event>? Events { get; set; }
    public virtual IEnumerable<Report>? Reports { get; set; }
             
    public virtual IEnumerable<BrokerUser>? BrokerUsers { get; set; }
    public virtual IEnumerable<BrokerExchange>? BrokerExchanges { get; set; }
}