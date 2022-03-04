using IM.Service.Data.Clients;
using IM.Service.Data.Domain.DataAccess;
using IM.Service.Data.Domain.Entities;
using IM.Service.Data.Services.DataFounders.Prices.Implementations;

namespace IM.Service.Data.Services.DataFounders.Prices;

public class PriceGrabber : DataGrabber
{
    public PriceGrabber(Repository<Price> repository, ILogger<PriceGrabber> logger, MoexClient moexClient, TdAmeritradeClient tdAmeritradeClient) 
        : base(new(StringComparer.InvariantCultureIgnoreCase)
        {
            { nameof(Enums.Sources.Moex), new MoexGrabber(repository, logger, moexClient) },
            { nameof(Enums.Sources.Tdameritrade), new TdameritradeGrabber(repository, logger, tdAmeritradeClient) }
        })
    {
    }
}