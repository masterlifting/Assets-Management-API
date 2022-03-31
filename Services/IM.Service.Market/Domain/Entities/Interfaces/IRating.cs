using IM.Service.Market.Domain.Entities.Catalogs;

namespace IM.Service.Market.Domain.Entities.Interfaces;

public interface IRating : IDataIdentity
{
    Status Status { get; set; }
    byte StatusId { get; set; }

    decimal? Result { get; set; }
}