using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data;

namespace IM.Service.Market.Services.Entity;

public sealed class SplitService
{
    public DataLoader<Split> Loader { get; }

    public SplitService(DataLoader<Split> loader) => Loader = loader;
}