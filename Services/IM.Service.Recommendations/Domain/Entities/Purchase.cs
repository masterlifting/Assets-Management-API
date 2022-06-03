using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Recommendations.Domain.Entities;

public class Purchase
{
    [Key]
    public int Id { get; set; }

    public string CompanyId { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "Decimal(18,4)")]
    public decimal Price { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal Plan { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Fact { get; set; }
}