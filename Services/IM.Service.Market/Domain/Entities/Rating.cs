using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Domain.Entities;

public class Rating : ICompanyIdentity, IDateIdentity
{
    [Key]
    public int Id { get; init; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Result { get; init; }

    public Company Company { get; init; } = null!;
    public string CompanyId { get; set; } = null!;

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    
    public TimeOnly Time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ResultPrice { get; init; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ResultReport { get; init; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ResultCoefficient { get; init; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ResultDividend { get; init; }
}