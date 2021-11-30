using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Company.Analyzer.DataAccess.Entities;

public class Rating
{
    [Key]
    public int Place { get; set; }

    public Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;


    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "Decimal(18,2)")]
    public decimal Result { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal AgregateResult { get; set; }
}