using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.RabbitServices;

using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.Services.PriceServices;

using System;
using System.Text.Json;
using System.Threading.Tasks;

using static IM.Services.Companies.Prices.Api.Enums;

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

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
        {
            if (entity == QueueEntities.Price && action == QueueActions.Download && RabbitHelper.TrySerialize(data, out Ticker ticker) && ticker is not null)
            {
                var prices = await priceLoader.LoadPricesAsync(ticker);
                if (prices.Length > 0)
                {
                    string sourceType = Enum.Parse<PriceSourceTypes>(ticker.SourceTypeId.ToString()).ToString();
                    var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Calculator);

                    for (int i = 0; i < prices.Length; i++)
                        publisher.PublishTask(
                            QueueNames.CompaniesAnalyzer
                            , QueueEntities.Price
                            , QueueActions.Calculate
                            , JsonSerializer.Serialize(new AnalyzerPriceDto
                            {
                                TickerName = ticker.Name,
                                Date = prices[i].Date,
                                SourceType = sourceType
                            }));
                }
            }

            return true;
        }
    }
}
