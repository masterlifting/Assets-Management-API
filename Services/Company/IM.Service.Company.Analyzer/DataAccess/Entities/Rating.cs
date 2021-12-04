using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Company.Analyzer.DataAccess.Entities;

public class Rating
{
    [Key]
    public int Id { get; init; }

    public Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;


    public int Place { get; set; }
    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    
    [Column(TypeName = "Decimal(18,4)")]
    public decimal Result { get; set; }


    [Column(TypeName = "Decimal(18,4)")]
    public decimal ResultPrice { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal ResultReport { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal ResultCoefficient { get; set; }
}