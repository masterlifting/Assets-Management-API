using System.Collections.Generic;
using IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class User
{
    public string Id { get; set; } = null!;

    public IEnumerable<BrokerUser>? BrokerUsers { get; set; }
}