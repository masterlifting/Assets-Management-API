using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.RabbitServices;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Services.Company.Prices.DataAccess.Entities;
using IM.Services.Company.Prices.Services.PriceServices;
using static IM.Services.Company.Prices.Enums;

namespace IM.Services.Company.Prices.Services.RabbitServices.Implementations
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
                    var sourceType = Enum.Parse<Enums.PriceSourceTypes>(ticker.SourceTypeId.ToString(),true).ToString();
                    var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Calculator);

                    foreach (var price in prices)
                        publisher.PublishTask(
                            QueueNames.CompaniesAnalyzer
                            , QueueEntities.Price
                            , QueueActions.Calculate
                            , JsonSerializer.Serialize(new AnalyzerPriceDto
                            {
                                TickerName = ticker.Name,
                                Date = price.Date,
                                SourceType = sourceType
                            }));
                }
            }

            return true;
        }
    }
}
