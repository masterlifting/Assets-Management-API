using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Domain.Entities;

public class Split : IDateIdentity, IDataIdentity
{
    public virtual Company Company { get; init; } = null!;
    public string CompanyId { get; set; } = null!;

    public virtual Source Source { get; init; } = null!;
    public byte SourceId { get; set; }

    public DateOnly Date { get; set; }

    public int Value { get; set; }
}