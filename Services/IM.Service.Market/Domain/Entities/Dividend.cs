using System.ComponentModel.DataAnnotations;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using IM.Service.Market.Domain.Entities.Catalogs;
using IM.Service.Market.Domain.Entities.Interfaces;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Domain.Entities;

public class Dividend : IDateIdentity, IRating
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; set; } = null!;

    public virtual Source Source { get; init; } = null!;
    [Range(1, byte.MaxValue)]
    public byte SourceId { get; set; }
    
    public DateOnly Date { get; set; }


    [Column(TypeName = "Decimal(18,4)")]
    public decimal Value { get; set; }

    public virtual Currency Currency { get; set; } = null!;
    [Range(1, byte.MaxValue)]
    public byte CurrencyId { get; set; }


    public virtual Status Status { get; set; } = null!;
    [Range(1, byte.MaxValue)]
    public byte StatusId { get; set; } = (byte)Statuses.New;
    [Column(TypeName = "Decimal(18,4)")]
    public decimal? Result { get; set; }
}