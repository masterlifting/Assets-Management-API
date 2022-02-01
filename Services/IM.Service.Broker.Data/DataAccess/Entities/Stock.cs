using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Stock
{
    [Key]
    public string Isin { get; init; } = null!;

    public Company Company { get; set; } = null!;
    public string CompanyId { get; set; } = null!;

    public Exchange Exchange { get; set; } = null!;
    public byte ExchangeId { get; set; }

    public IEnumerable<Transaction>? Transactions { get; set; }
}