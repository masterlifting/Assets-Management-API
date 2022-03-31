namespace IM.Service.Market.Domain.Entities.Interfaces;

public interface ISourceIdentity
{
    Source Source { get; init; }
    byte SourceId { get; set; }
}