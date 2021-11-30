using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Company.Analyzer.DataAccess.Entities;

public class RatingData
{
    [Key]
    public int Id { get; set; }

    public Company Company { get; set; } = null!;
    public string CompanyId { get; set; } = null!;
    
    public AnalyzedEntityType EntityType { get; set; } = null!;
    public byte AnalyzedEntityTypeId { get; set; }


    [Column(TypeName = "Decimal(18,4)")]
    public decimal Result { get; set; }

    [Column(TypeName = "Date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;
}