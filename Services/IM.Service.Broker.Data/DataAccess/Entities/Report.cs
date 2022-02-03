using System;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Report
{
    public Account Account { get; init; } = null!;
    public int AccountId { get; init; }

    public string Name { get; init; } = null!;
    
    public DateOnly DateStart { get; init; }
    public DateOnly DateEnd { get; init; }

    public string ContentType { get; set; } = null!;
    public byte[] Payload { get; set; } = null!;
}