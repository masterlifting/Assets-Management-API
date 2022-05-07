using System;

namespace IM.Service.Portfolio.Domain.Entities;

public class Report
{
    public string Name { get; init; } = null!;

    public DateOnly DateStart { get; set; }
    public DateOnly DateEnd { get; set; }

    public string ContentType { get; set; } = null!;
    public byte[] Payload { get; set; } = null!;

    public virtual Account Account { get; init; } = null!;
    public string AccountUserId { get; set; } = null!;
    public byte AccountBrokerId { get; set; }
    public string AccountName { get; set; } = null!;
}