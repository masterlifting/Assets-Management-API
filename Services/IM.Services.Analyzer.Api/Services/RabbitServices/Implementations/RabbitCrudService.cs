using CommonServices.RabbitServices;

using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.DataAccess.Repository;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.RabbitServices.Implementations
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

        private static async Task<bool> GetTickerResultAsync(QueueActions action, string data, IServiceScope scope)
        {
            if (!RabbitService.TrySerialize(data, out Ticker? ticker) && ticker is null)
                return false;

            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<Ticker>>();

            if (repository is null)
                return false;

            return action == QueueActions.delete
                ? await repository.DeleteAsync(data, data)
                : await GetActionAsync(repository!, action, ticker!, ticker!.Name);
        }

        private static async Task<bool> GetActionAsync<T>(EntityRepository<T> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.create => await repository.CreateAsync(data, value),
            QueueActions.update => await repository.UpdateAsync(data, value),
            _ => true
        };
    }
}
