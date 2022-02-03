using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

namespace IM.Service.Company.Analyzer.DataAccess.Entities;

public class Rating : ICompanyDateIdentity
{
    [Key]
    public int Id { get; init; }
    
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Result { get; set; }

    public string CompanyId { get; init; } = null!;
    public Company Company { get; init; } = null!;

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public TimeOnly Time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ResultPrice { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ResultReport { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ResultCoefficient { get; set; }
}