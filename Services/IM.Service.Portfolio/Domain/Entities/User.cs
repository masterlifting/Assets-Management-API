using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IM.Service.Portfolio.Domain.Entities;

public class User
{
    [Key, Required, StringLength(100)]
    public string Id { get; init; } = null!;
    [Required, StringLength(500)]
    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual IEnumerable<Account>? Accounts { get; set; } = null!;
    [JsonIgnore]
    public virtual IEnumerable<Deal>? Deals { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Event>? Events { get; set; }
}