using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.Background.PriceUpdaterBackgroundServices.Interfaces
{
    public interface IPriceUpdater
    {
        Task<int> UpdatePricesAsync();
    }
}
