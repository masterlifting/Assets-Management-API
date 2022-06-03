using System;
using IM.Service.Portfolio.Domain.Entities.Catalogs;

namespace IM.Service.Portfolio.Domain.Entities;

public class Report
{
    public string Id { get; init; } = null!;

    public virtual Broker Broker { get; init; } = null!;
    public byte BrokerId { get; init; }

    public DateOnly DateStart { get; set; }
    public DateOnly DateEnd { get; set; }

    public string ContentType { get; set; } = null!;
    public byte[] Payload { get; set; } = null!;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
}