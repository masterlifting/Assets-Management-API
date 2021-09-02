using CommonServices.RabbitServices;
using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope) => entity switch
        {
            QueueEntities.ticker => await GetTickerResultAsync(action, data, scope),
            _ => true
        };

        private static async Task<bool> GetTickerResultAsync(QueueActions action, string data, IServiceScope scope)
        {
            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<Ticker, AnalyzerContext>>();

            return repository is not null
                && (action == QueueActions.delete
                    ? await repository.DeleteAsync(data, data)
                    : RabbitHelper.TrySerialize(data, out Ticker? ticker) && await GetActionAsync(repository!, action, ticker!, ticker!.Name));
        }

        private static async Task<bool> GetActionAsync<T>(EntityRepository<T, AnalyzerContext> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.create => await repository.CreateAsync(data, value),
            QueueActions.update => await repository.UpdateAsync(data, value),
            _ => true
        };
    }
}
