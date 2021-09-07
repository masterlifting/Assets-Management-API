using CommonServices.Models.Dto.CompaniesPricesService;
using CommonServices.RabbitServices;

using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.DataAccess.Repository;

using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        private readonly string rabbitConnectionString;
        private readonly PricesRepository<Ticker> repository;

        public RabbitCrudService(string rabbitConnectionString, PricesRepository<Ticker> repository)
        {
            this.rabbitConnectionString = rabbitConnectionString;
            this.repository = repository;
        }

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
        {
            QueueEntities.ticker => await GetTickerResultAsync(action, data),
            _ => true
        };

        private async Task<bool> GetTickerResultAsync(QueueActions action, string data)
        {
            if (repository is null)
                return false;

            if (action == QueueActions.delete)
                return await repository.DeleteAsync(data, data);

            if (!RabbitHelper.TrySerialize(data, out CompaniesPricesTickerDto ticker) && ticker is null)
                return false;

            if (await GetActionAsync(repository, action, new Ticker(ticker), ticker.Name))
            {
                var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.loader);
                publisher.PublishTask(QueueNames.companiesprices, QueueEntities.price, QueueActions.download, JsonSerializer.Serialize(ticker));

                return true;
            }

            return false;
        }

        private static async Task<bool> GetActionAsync<T>(PricesRepository<T> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.create => await repository.CreateAsync(data, value) is not null,
            QueueActions.update => await repository.CreateOrUpdateAsync(data, value),
            _ => true
        };
    }
}
