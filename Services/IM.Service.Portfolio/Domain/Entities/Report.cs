using System;
using System.ComponentModel.DataAnnotations;

using IM.Service.Portfolio.Domain.Entities.Catalogs;

namespace IM.Service.Portfolio.Domain.Entities;

public class Report
{
    [Required, StringLength(200, MinimumLength = 3)]
    public string Id { get; init; } = null!;

    public virtual Account Account { get; init; } = null!;
    public int AccountId { get; init; }

    public virtual Provider Provider { get; init; } = null!;
    public int ProviderId { get; init; } = (int)Enums.Providers.Default;

    public DateOnly DateStart { get; set; }
    public DateOnly DateEnd { get; set; }
    
    [Required, StringLength(30)]
    public string ContentType { get; set; } = null!;
    public byte[] Payload { get; set; } = null!;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
}