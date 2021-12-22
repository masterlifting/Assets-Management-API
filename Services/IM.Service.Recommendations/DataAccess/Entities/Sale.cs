using System;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Recommendations.DataAccess.Entities;

public class Sale
{
    [Key]
    public int Id { get; set; }

    public string CompanyId { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public int Lot { get; set; }
    public decimal Price { get; set; }
    public int Percent { get; set; }
}