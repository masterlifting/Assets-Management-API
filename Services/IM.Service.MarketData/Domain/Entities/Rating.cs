using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.MarketData.Domain.Entities.Interfaces;

namespace IM.Service.MarketData.Domain.Entities;

public class Rating : ICompanyIdentity, IDateIdentity
{
    [Key]
    public int Id { get; init; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Result { get; set; }

    public Company Company { get; init; } = null!;
    public string CompanyId { get; set; } = null!;

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    
    public TimeOnly Time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ResultPrice { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ResultReport { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ResultCoefficient { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ResultDividend { get; set; }
}