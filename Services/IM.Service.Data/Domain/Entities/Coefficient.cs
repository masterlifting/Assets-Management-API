using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Domain.Entities.Catalogs;
using IM.Service.Data.Domain.Entities.Interfaces;

using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Data.Domain.Entities;

public class Coefficient : IQuarterIdentity, IDataIdentity, IRating
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;

    public Source Source { get; init; } = null!;
    public byte SourceId { get; init; }

    public int Year { get; init; }
    public byte Quarter { get; init; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal Pe { get; init; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal Pb { get; init; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal DebtLoad { get; init; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal Profitability { get; init; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal Roa { get; init; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal Roe { get; init; }
    [Column(TypeName = "Decimal(18,4)")]
    public decimal Eps { get; init; }

    public Status Status { get; set; } = null!;
    public byte StatusId { get; set; }
    
    public decimal? Result { get; set; }
}