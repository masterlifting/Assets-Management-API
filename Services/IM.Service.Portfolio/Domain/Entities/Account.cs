using IM.Service.Portfolio.Domain.Entities.Catalogs;

using System;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Portfolio.Domain.Entities;

public class Account
{
    [Key]
    public int Id { get; set; }

    public string Name { get; init; } = null!;

    public virtual User User { get; init; } = null!;
    public string UserId { get; init; } = null!;

    public virtual Broker Broker { get; init; } = null!;
    public byte BrokerId { get; init; }

    public DateOnly DateCreate { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
}