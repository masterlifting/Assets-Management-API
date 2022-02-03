using System;
using System.Collections.Generic;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Account
{
    public int Id { get; init; }

    public string Name { get; init; } = null!;
    public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public IEnumerable<Transaction>? Transactions { get; set; }
    public IEnumerable<Report>? Reports { get; set; }

    public Broker Broker { get; set; } = null!;
    public byte BrokerId { get; init; }

    public User User { get; set; } = null!;
    public string UserId { get; init; } = null!;
}