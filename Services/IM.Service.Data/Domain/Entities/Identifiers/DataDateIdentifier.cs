using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Domain.Entities.Interfaces;

namespace IM.Service.Data.Domain.Entities.Identifiers;

public class DataDateIdentifier : IDataIdentity, IDateIdentity
{
    public Company Company { get; init; } = null!;
    public string CompanyId { get; init; } = null!;
    public Source Source { get; init; } = null!;
    public byte SourceId { get; init; }
    public DateOnly Date { get; set; }
}