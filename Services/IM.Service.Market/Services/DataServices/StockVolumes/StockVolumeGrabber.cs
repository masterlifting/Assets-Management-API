using Microsoft.Extensions.Logging;

using System;
using IM.Service.Market.Clients;
using IM.Service.Market.DataAccess.Entities;
using IM.Service.Market.DataAccess.Repositories;
using IM.Service.Market.Services.DataServices.StockVolumes.Implementations;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.DataServices.StockVolumes;

public class StockVolumeGrabber : DataGrabber
{
    public StockVolumeGrabber(Repository<StockVolume> repository, ILogger<StockVolumeGrabber> logger,
        InvestingClient investingClient)
        : base(new(StringComparer.InvariantCultureIgnoreCase)
        {
            {nameof(Enums.Sources.Investing), new InvestingGrabber(repository, logger, investingClient)}
        })
    {
    }
}