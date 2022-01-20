using IM.Service.Company.Data.Clients.Report;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Services.DataServices.StockVolumes.Implementations;

using Microsoft.Extensions.Logging;

using System;

using static IM.Service.Company.Data.Enums;

namespace IM.Service.Company.Data.Services.DataServices.StockVolumes;

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