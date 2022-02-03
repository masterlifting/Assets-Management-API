using System;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Transaction
{
    public long Id { get; set; }

    public DateTime DateTime { get; set; }

    public decimal Cost { get; set; }
    public decimal Value { get; set; }
    
    public string? Info { get; set; }

    public Account Account { get; set; } = null!;
    public int AccountId { get; set; }

    public Stock? Stock { get; set; }
    public string? StockIsin { get; set; }

    public TransactionAction TransactionAction { get; set; } = null!;
    public byte TransactionActionId { get; set; }

    public Currency Currency { get; set; } = null!;
    public byte CurrencyId { get; set; }
}