using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Domain.Entities.Catalogs;
using IM.Service.Data.Domain.Entities.Interfaces;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Data.Domain.Entities;

public class Report : IQuarterIdentity, IDataIdentity, IRating
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;

    public Source Source { get; init; } = null!;
    public byte SourceId { get; init; }

    public int Year { get; init; }
    public byte Quarter { get; init; }

    [Range(1, int.MaxValue)]
    public int Multiplier { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Revenue { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ProfitNet { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ProfitGross { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? CashFlow { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Asset { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Turnover { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? ShareCapital { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Obligation { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    public decimal? LongTermDebt { get; set; }


    public Status Status { get; set; } = null!;
    public byte StatusId { get; set; }

    public decimal? Result { get; set; }
}