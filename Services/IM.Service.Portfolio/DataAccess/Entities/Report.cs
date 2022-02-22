using System;
using IM.Service.Portfolio.DataAccess.Entities.Catalogs;

namespace IM.Service.Portfolio.DataAccess.Entities;

public class Report
{
    public string Name { get; init; } = null!;

    public DateOnly DateStart { get; init; }
    public DateOnly DateEnd { get; init; }

    public string ContentType { get; set; } = null!;
    public byte[] Payload { get; set; } = null!;

    public virtual Account Account { get; init; } = null!;
    public string AccountName { get; init; } = null!;

    public virtual Broker Broker { get; set; } = null!;
    public byte BrokerId { get; set; }

    public virtual User User { get; set; } = null!;
    public string UserId { get; set; } = null!;
}