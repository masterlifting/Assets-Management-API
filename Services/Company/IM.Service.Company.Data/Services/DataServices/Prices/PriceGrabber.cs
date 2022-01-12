using IM.Service.Company.Data.Clients.Price;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Services.DataServices.Prices.Implementations;

using Microsoft.Extensions.Logging;

using System;

namespace IM.Service.Company.Data.Services.DataServices.Prices;

public class PriceGrabber : DataGrabber
{
    public PriceGrabber(Repository<Price> repository, ILogger<PriceGrabber> logger, MoexClient moexClient, TdAmeritradeClient tdAmeritradeClient) 
        : base(new(StringComparer.InvariantCultureIgnoreCase)
        {
            { nameof(Enums.SourceTypes.Moex), new MoexGrabber(repository, logger, moexClient) },
            { nameof(Enums.SourceTypes.Tdameritrade), new TdameritradeGrabber(repository, logger, tdAmeritradeClient) }
        })
    {
    }
}