using IM.Service.Shared.Models.Entity.Interfaces;
using IM.Service.Market.Domain.Entities.Interfaces;

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IM.Service.Market.Domain.Entities;

public class Rating : ICompanyIdentity, IDateIdentity
{
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Result { get; set; }

    [JsonIgnore]
    public virtual Company Company { get; init; } = null!;
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