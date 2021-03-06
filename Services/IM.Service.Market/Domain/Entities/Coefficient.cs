using System.ComponentModel.DataAnnotations;
using IM.Service.Shared.Models.Entity.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using IM.Service.Market.Domain.Entities.Catalogs;
using IM.Service.Market.Domain.Entities.Interfaces;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Domain.Entities;

public class Coefficient : IQuarterIdentity, IRating
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; set; } = null!;

    public virtual Source Source { get; init; } = null!;
    public byte SourceId { get; set; }

    [Range(1900, 3000)]
    public int Year { get; set; }
    [Range(1, 4)]
    public byte Quarter { get; set; }


    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Pe { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Pb { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? DebtLoad { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Profitability { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Roa { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Roe { get; set; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Eps { get; set; }


    public virtual Status Status { get; set; } = null!;
    [Range(1, byte.MaxValue)]
    public byte StatusId { get; set; } = (byte)Statuses.New;
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Result { get; set; }
}