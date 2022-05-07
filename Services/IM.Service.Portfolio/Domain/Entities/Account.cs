using System;
using System.Collections.Generic;
using IM.Service.Portfolio.Domain.Entities.Catalogs;

namespace IM.Service.Portfolio.Domain.Entities;

public class Account
{
    public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public string Name { get; init; } = null!;

    public virtual Broker Broker { get; set; } = null!;
    public byte BrokerId { get; init; }

    public virtual User User { get; set; } = null!;
    public string UserId { get; init; } = null!;

    public virtual IEnumerable<Deal>? Deals { get; set; }
    public virtual IEnumerable<Event>? Events { get; set; }
    public virtual IEnumerable<Report>? Reports { get; set; }
}