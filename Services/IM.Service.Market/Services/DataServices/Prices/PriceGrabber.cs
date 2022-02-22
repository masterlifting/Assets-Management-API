using Microsoft.Extensions.Logging;

using System;
using IM.Service.Market.Clients;
using IM.Service.Market.DataAccess.Entities;
using IM.Service.Market.DataAccess.Repositories;
using IM.Service.Market.Services.DataServices.Prices.Implementations;

namespace IM.Service.Market.Services.DataServices.Prices;

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