using System.Collections.Generic;
using IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Account
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public BrokerUser BrokerUser { get; set; } = null!;
    public int BrokerUserId { get; set; }

    public IEnumerable<Transaction>? Transactions { get; set; }
    public IEnumerable<Report>? Reports { get; set; }
}