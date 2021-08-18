using CommonServices.RabbitServices;
using CommonServices.RabbitServices.Configuration;

using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.DataAccess.Repository;

using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        private readonly RabbitService queueService;
        public RabbitCrudService(RabbitService queueService) => this.queueService = queueService;
        
        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope) => entity switch
        {
            QueueEntities.ticker => await GetTickerResultAsync(action, data, scope),
            QueueEntities.reportsource => await GetReportSourceResultAsync(action, data, scope),
            _ => true
        };

        private static async Task<bool> GetTickerResultAsync(QueueActions action, string data, IServiceScope scope)
        {
            if (!RabbitService.TrySerialize(data, out Ticker ticker) && ticker is null)
                return false;

            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<Ticker>>();

            if (repository is null)
                return false;

            return action == QueueActions.delete
                ? await repository.DeleteAsync(data, data)
                : await GetActionAsync(repository, action, ticker, ticker.Name);
        }
        private async Task<bool> GetReportSourceResultAsync(QueueActions action, string data, IServiceScope scope)
        {
            bool result = false;
            
            if (!RabbitService.TrySerialize(data, out ReportSource source) && source is null)
                return result;

            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<ReportSource>>();

            if (repository is null)
                return result;

            if(action == QueueActions.delete)
                result = await repository.DeleteAsync(int.Parse(data), data);
            else
            {
                result = await GetActionAsync(repository, action, source, source.Value);

                if (result)
                    queueService.GetPublisher(QueueExchanges.parser).PublishTask(QueueEntities.report, QueueActions.download, JsonSerializer.Serialize(source));
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
