using System;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Report
{
    public Account Account { get; init; } = null!;
    public int AccountId { get; init; }
    public string FileName { get; init; } = null!;
    
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public string FileContentType { get; set; } = null!;
    public byte[] FilePayload { get; set; } = null!;
}