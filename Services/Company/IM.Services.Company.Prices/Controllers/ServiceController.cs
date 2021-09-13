using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Services.Company.Prices.Services.PriceServices;

namespace IM.Services.Company.Prices.Controllers
{
    [ApiController, Route("[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly PriceLoader priceLoader;
        public ServiceController(PriceLoader priceLoader) => this.priceLoader = priceLoader;

        [HttpPost("update/")]
        public async Task<string> UpdatePrices()
        {
            int updatedCount = await priceLoader.LoadPricesAsync();
            return  $"updated prices count: {updatedCount}";
        }
    }
}