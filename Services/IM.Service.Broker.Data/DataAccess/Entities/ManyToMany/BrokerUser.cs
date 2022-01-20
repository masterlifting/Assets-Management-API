using System.Collections.Generic;

namespace IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;

public class BrokerUser
{
    public int Id { get; set; }

    public User User { get; set; } = null!;
    public string UserId { get; set; } = null!;

    public Broker Broker { get; set; } = null!;
    public byte BrokerId { get; set; }

    public IEnumerable<Account>? Accounts { get; set; } = null!;
}