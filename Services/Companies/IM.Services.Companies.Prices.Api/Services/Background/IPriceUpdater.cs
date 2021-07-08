using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.Background
{
    public interface IPriceUpdater
    {
        Task<int> UpdatePricesAsync();
    }
}
