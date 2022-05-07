using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Domain.Entities.Catalogs;

namespace IM.Service.Portfolio.Domain.Entities.ManyToMany;

public class BrokerUser
{
    public virtual Broker Broker { get; init; } = null!;
    public byte BrokerId { get; init; }

    public virtual User User { get; init; } = null!;
    public string UserId { get; init; } = null!;
}