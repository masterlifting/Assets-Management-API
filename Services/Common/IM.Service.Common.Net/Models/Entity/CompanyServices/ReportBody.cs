using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Common.Net.Models.Entity.CompanyServices;

public abstract class ReportBody : SourceTypeBody
{
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
}