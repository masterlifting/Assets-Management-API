using IM.Service.Common.Net.Models.Entity.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IM.Service.MarketData.Domain.Entities.Catalogs;
using IM.Service.MarketData.Domain.Entities.Interfaces;

namespace IM.Service.MarketData.Domain.Entities;

public class Report : IQuarterIdentity, IDataIdentity, IRating
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; set; } = null!;

    public Source Source { get; init; } = null!;
    public byte SourceId { get; set; }

    [Range(1900, 3000)]
    public int Year { get; set; }
    
    [Range(1, 4)]
    public byte Quarter { get; set; }


    [Range(1, int.MaxValue)]
    public int Multiplier { get; set; }

    public Currency Currency { get; set; } = null!;
    [Range(1, byte.MaxValue)]
    public byte CurrencyId { get; set; }


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
    [Range(1, byte.MaxValue)]
    public byte StatusId { get; set; } = (byte)Enums.Statuses.Ready;

    public decimal? Result { get; set; }
}