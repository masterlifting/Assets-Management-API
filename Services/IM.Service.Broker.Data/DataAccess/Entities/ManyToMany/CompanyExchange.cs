using System.Collections.Generic;

namespace IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;

public class CompanyExchange
{
    public int Id { get; set; }

    public Company Company { get; set; } = null!;
    public string CompanyId { get; set; } = null!;

    public  Exchange Exchange { get; set; } = null!;
    public byte  ExchangeId { get; set; }

    public IEnumerable<Stock>? Stocks { get; set; }
}