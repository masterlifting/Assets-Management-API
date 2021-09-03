using CommonServices.Models.Dto.CompaniesPricesService;
using CommonServices.RabbitServices;
using CommonServices.RepositoryService;

using IM.Services.Companies.Prices.Api.DataAccess;
using IM.Services.Companies.Prices.Api.DataAccess.Entities;

using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        private readonly string rabbitConnectionString;
        private readonly EntityRepository<Ticker, PricesContext> repository;

        public RabbitCrudService(string rabbitConnectionString, EntityRepository<Ticker, PricesContext> repository)
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

        private static async Task<bool> GetActionAsync<T>(EntityRepository<T, PricesContext> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.create => await repository.CreateAsync(data, value),
            QueueActions.update => await repository.UpdateAsync(data, value),
            _ => true
        };
    }
}
