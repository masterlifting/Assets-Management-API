using IM.Services.Companies.Prices.Api.Services.Background;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly IPriceUpdater priceUpdater;
        public ServiceController(IPriceUpdater priceUpdater) => this.priceUpdater = priceUpdater;

        [HttpPost("update/")]
        public async Task<string> UpdatePrices()
        {
            int updatedCount = await priceUpdater.UpdatePricesAsync();
            return  $"updated prices count: {updatedCount}";
        }
    }
}