using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Domain.Entities.Identifiers;

public class DataDateIdentifier : IDataIdentity, IDateIdentity
{
    public Company Company { get; init; } = null!;
    public string CompanyId { get; set; } = null!;
    public Source Source { get; init; } = null!;
    public byte SourceId { get; set; }
    public DateOnly Date { get; set; }
}