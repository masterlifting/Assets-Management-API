using System.ComponentModel.DataAnnotations;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using IM.Service.MarketData.Domain.Entities.Catalogs;
using IM.Service.MarketData.Domain.Entities.Interfaces;

namespace IM.Service.MarketData.Domain.Entities;

public class Price : IDateIdentity, IDataIdentity, IRating
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; set; } = null!;

    public Source Source { get; init; } = null!;
    [Range(1, byte.MaxValue)]
    public byte SourceId { get; set; }

    public DateOnly Date { get; set; }


    [Column(TypeName = "Decimal(18,4)")]
    public decimal Value { get; set; }
    
    [Column(TypeName = "Decimal(18,4)")]
    public decimal ValueTrue { get; set; }
    
    public Currency Currency { get; set; } = null!;
    [Range(1, byte.MaxValue)]
    public byte CurrencyId { get; set; }


    public Status Status { get; set; } = null!;
    [Range(1, byte.MaxValue)]
    public byte StatusId { get; set; } = (byte)Enums.Statuses.Ready;

    public decimal? Result { get; set; }
}