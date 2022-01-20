using System.Collections.Generic;
using IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;
using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Exchange : CommonEntityType
{
    public IEnumerable<BrokerExchange>? BrokerExchanges { get; set; }
    public IEnumerable<CompanyExchange>? CompanyExchanges { get; set; }
}