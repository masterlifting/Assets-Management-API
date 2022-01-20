using System.Collections.Generic;
using IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Stock
{
    public int Id { get; set; }

    public string Isin { get; set; } = null!;
    public string Ticker { get; set; } = null!;

    public decimal Lot { get; set; }

    public CompanyExchange CompanyExchange { get; set; } = null!;
    public int CompanyExchangeId { get; set; }

    public IEnumerable<Transaction>? Transactions { get; set; }
}