using CommonServices.RabbitServices;

using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.DataAccess.Repository;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        private readonly RabbitService rabbitService;
        public RabbitCrudService(RabbitService rabbitService) => this.rabbitService = rabbitService;

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope) => entity switch
        {
            QueueEntities.ticker => await GetTickerResultAsync(action, data, scope),
            _ => true
        };

        private async Task<bool> GetTickerResultAsync(QueueActions action, string data, IServiceScope scope)
        {
            bool result = false;
            
            if (!RabbitService.TrySerialize(data, out Ticker ticker) && ticker is null)
                return result;

            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<Ticker>>();

            if (repository is null)
                return result;

            if (action == QueueActions.delete)
                result = await repository.DeleteAsync(data, data);
            else
            {
                result = await GetActionAsync(repository, action, ticker, ticker.Name);

                if (result)
                    rabbitService.GetPublisher(QueueExchanges.loader).PublishTask(QueueEntities.price, QueueActions.download, ticker.PriceSourceTypeId.ToString());
            }

            return result;
        }

        private static async Task<bool> GetActionAsync<T>(EntityRepository<T> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.create => await repository.CreateAsync(data, value),
            QueueActions.update => await repository.UpdateAsync(data, value),
            _ => true
        };
    }
}
