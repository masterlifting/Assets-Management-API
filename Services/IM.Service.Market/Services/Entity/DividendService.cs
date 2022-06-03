using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data;
using IM.Service.Market.Services.Helpers;

namespace IM.Service.Market.Services.Entity;

public sealed class DividendService : StatusChanger<Dividend>
{
    public DataLoader<Dividend> Loader { get; }

    public DividendService(Repository<Dividend> reportRepo, DataLoader<Dividend> loader) : base(reportRepo)
    {
        Loader = loader;
    }
}