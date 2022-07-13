using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

[Index(nameof(Name), IsUnique = true)]
public class Provider
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    [Required, StringLength(100, MinimumLength = 3)]
    public string Name { get; init; } = null!;

    [StringLength(200)]
    public string? Description { get; set; }

    [JsonIgnore]
    public virtual IEnumerable<Deal>? Deals { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Event>? Events { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Account>? Accounts { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Report>? Reports { get; set; }
}