using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Domain.Entities.Catalogs;
using IM.Service.Data.Domain.Entities.Interfaces;

using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Data.Domain.Entities;

public class Price : IDateIdentity, IDataIdentity, IRating
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;

    public Source Source { get; init; } = null!;
    public byte SourceId { get; init; }

    public DateOnly Date { get; set; }



    [Column(TypeName = "Decimal(18,4)")]
    public decimal Value { get; set; }


    public Status Status { get; set; } = null!;
    public byte StatusId { get; set; }

    public decimal? Result { get; set; }
}