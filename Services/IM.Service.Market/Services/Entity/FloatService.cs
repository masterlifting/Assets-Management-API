using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data;

namespace IM.Service.Market.Services.Entity;

public sealed class FloatService
{
    public DataLoader<Float> Loader { get; }

    public FloatService(DataLoader<Float> loader)
    {
        Loader = loader;
    }
}