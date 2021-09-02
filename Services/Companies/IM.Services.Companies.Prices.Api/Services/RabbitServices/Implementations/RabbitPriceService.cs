
using CommonServices.Models.AnalyzerService;
using CommonServices.RabbitServices;

using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.Services.PriceServices;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.RabbitServices.Implementations
{
    public class RabbitPriceService : IRabbitActionService
    {
        private readonly PriceLoader priceLoader;
        private readonly string rabbitConnectionString;

        public RabbitPriceService(PriceLoader priceLoader, string rabbitConnectionString)
        {
            this.priceLoader = priceLoader;
            this.rabbitConnectionString = rabbitConnectionString;
        }

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope)
        {
            if (entity == QueueEntities.price && action == QueueActions.download && RabbitHelper.TrySerialize(data, out Ticker ticker) && ticker is not null)
            {
                var prices = await priceLoader.LoadPricesAsync(ticker);
                if (prices.Length > 0)
                {
                    var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.calculator);
                    for (int i = 0; i < prices.Length; i++)
                        publisher.PublishTask(
                            QueueNames.companiesanalyzer
                            , QueueEntities.price
                            , QueueActions.calculate
                            , JsonSerializer.Serialize(new AnalyzerPriceDto
                            {
                                TickerName = ticker.Name,
                                Date = prices[i].Date,
                                PriceSourceTypeId = ticker.PriceSourceTypeId
                            }));
                }
            }
            else
                Console.WriteLine(nameof(RabbitPriceService) + " error!");

            return true;
        }
    }
}
