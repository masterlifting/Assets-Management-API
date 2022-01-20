namespace IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;

public class BrokerExchange
{
    public int Id { get; set; }

    public Broker Broker { get; set; } = null!;
    public byte BrokerId { get; set; }

    public Exchange Exchange { get; set; } = null!;
    public byte ExchangeId { get; set; }
}