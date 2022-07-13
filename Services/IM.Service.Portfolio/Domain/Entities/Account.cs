using IM.Service.Portfolio.Domain.Entities.Catalogs;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IM.Service.Portfolio.Domain.Entities;

public class Account
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, StringLength(200, MinimumLength = 3)]
    public string Name { get; init; } = null!;

    public virtual User User { get; init; } = null!;
    public string UserId { get; init; } = null!;

    public virtual Provider Provider { get; init; } = null!;
    public int ProviderId { get; init; } = (int) Enums.Providers.Default;

    public DateOnly DateCreate { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [JsonIgnore]
    public virtual IEnumerable<Report>? Reports { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Deal>? Deals { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Event>? Events { get; set; }
}