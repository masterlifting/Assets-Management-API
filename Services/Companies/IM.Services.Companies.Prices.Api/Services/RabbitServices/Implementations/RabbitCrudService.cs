using CommonServices.RabbitServices;
using CommonServices.RepositoryService;

using IM.Services.Companies.Prices.Api.DataAccess;
using IM.Services.Companies.Prices.Api.DataAccess.Entities;

using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        private readonly string rabbitConnectionString;
        public RabbitCrudService(string rabbitConnectionString) => this.rabbitConnectionString = rabbitConnectionString;

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope) => entity switch
        {
            QueueEntities.ticker => await GetTickerResultAsync(action, data, scope),
            _ => true
        };

        private async Task<bool> GetTickerResultAsync(QueueActions action, string data, IServiceScope scope)
        {
            if (!RabbitHelper.TrySerialize(data, out Ticker ticker) && ticker is null)
                return false;

            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<Ticker,PricesContext>>();

            if (repository is null)
                return false;

            if (action == QueueActions.delete)
                return await repository.DeleteAsync(data, data);


            var result = await GetActionAsync(repository, action, ticker, ticker.Name);

            if (result)
            {
                var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.loader);
                publisher.PublishTask(QueueNames.companiesprices, QueueEntities.price, QueueActions.download, JsonSerializer.Serialize(ticker));
            }

            return result;
        }

        private static async Task<bool> GetActionAsync<T>(EntityRepository<T,PricesContext> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.create => await repository.CreateAsync(data, value),
            QueueActions.update => await repository.UpdateAsync(data, value),
            _ => true
        };
    }
}
