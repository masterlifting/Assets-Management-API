using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Domain.Entities.Interfaces;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Data.Domain.Entities;

public class Rating : IDateIdentity, IDataIdentity
{
    [Key]
    public int Id { get; init; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Result { get; set; }

    public Company Company { get; init; } = null!;
    public string CompanyId { get; set; } = null!;
    public Source Source { get; init; } = null!;
    public byte SourceId { get; set; }

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