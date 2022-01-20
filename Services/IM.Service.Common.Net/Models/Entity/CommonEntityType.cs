using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Common.Net.Models.Entity;

[Index(nameof(Name), IsUnique = true)]
public abstract class CommonEntityType
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public byte Id { get; init; }

    [Required, StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? Description { get; set; }
}