using System.Collections.Generic;
using IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;
using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Broker : CommonEntityType
{
    public IEnumerable<BrokerUser>? BrokerUsers { get; set; }
    public IEnumerable<BrokerExchange>? BrokerExchanges { get; set; }
}