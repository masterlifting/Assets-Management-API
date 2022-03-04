using IM.Service.Data.Clients;
using IM.Service.Data.Domain.DataAccess;
using IM.Service.Data.Domain.Entities;
using IM.Service.Data.Services.DataFounders.Floats.Implementations;

namespace IM.Service.Data.Services.DataFounders.Floats;

public class StockVolumeGrabber : DataGrabber
{
    public StockVolumeGrabber(Repository<Float> repository, ILogger<StockVolumeGrabber> logger,
        InvestingClient investingClient)
        : base(new(StringComparer.InvariantCultureIgnoreCase)
        {
            {nameof(Enums.Sources.Investing), new InvestingGrabber(repository, logger, investingClient)}
        })
    {
    }
}