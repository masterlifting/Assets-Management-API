using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.RabbitServices;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.Services.PriceServices;
using static IM.Service.Company.Prices.Enums;
// ReSharper disable InvertIf

namespace IM.Service.Company.Prices.Services.RabbitServices.Implementations
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
            if (entity == QueueEntities.Price 
                && action == QueueActions.GetData 
                && RabbitHelper.TrySerialize(data, out Ticker? ticker))
            {
                var prices = await priceLoader.LoadPricesAsync(ticker!);
                if (prices.Length > 0)
                {
                    var sourceType = Enum.Parse<PriceSourceTypes>(ticker!.SourceTypeId.ToString(),true).ToString();
                    var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);

                    foreach (var price in prices)
                        publisher.PublishTask(
                            QueueNames.CompanyAnalyzer
                            , QueueEntities.Price
                            , QueueActions.GetLogic
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
