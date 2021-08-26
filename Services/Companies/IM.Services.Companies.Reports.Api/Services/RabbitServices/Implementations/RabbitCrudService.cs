using CommonServices.RabbitServices;
using CommonServices.RepositoryService;

using IM.Services.Companies.Reports.Api.DataAccess;
using IM.Services.Companies.Reports.Api.DataAccess.Entities;

using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        private readonly string rabbitConnectionString;

        public RabbitCrudService(string rabbitConnectionString)
        {
            this.rabbitConnectionString = rabbitConnectionString;
        }

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope) => entity switch
        {
            QueueEntities.ticker => await GetTickerResultAsync(action, data, scope),
            QueueEntities.reportsource => await GetReportSourceResultAsync(action, data, scope),
            _ => true
        };

        private static async Task<bool> GetTickerResultAsync(QueueActions action, string data, IServiceScope scope)
        {
            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<Ticker, ReportsContext>>();

            if (repository is null)
                return false;

            if (action == QueueActions.delete)
                return await repository.DeleteAsync(data, data);

            if (!RabbitHelper.TrySerialize(data, out Ticker ticker) && ticker is null)
                return false;

            return await GetActionAsync(repository, action, ticker, ticker.Name);
        }
        private async Task<bool> GetReportSourceResultAsync(QueueActions action, string data, IServiceScope scope)
        {
            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<ReportSource, ReportsContext>>();

            if (repository is null)
                return false;

            if (action == QueueActions.delete)
                return await repository.DeleteAsync(int.Parse(data), data);

            if (!RabbitHelper.TrySerialize(data, out ReportSource source) && source is null)
                return false;

            var result = await GetActionAsync(repository, action, source, source.Value);

            if (result)
            {
                var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.loader);
                publisher.PublishTask(QueueNames.companiesreports, QueueEntities.report, QueueActions.download, JsonSerializer.Serialize(source));
            }

            return result;
        }

        private static async Task<bool> GetActionAsync<T>(EntityRepository<T, ReportsContext> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.create => await repository.CreateAsync(data, value),
            QueueActions.update => await repository.UpdateAsync(data, value),
            _ => true
        };
    }
}
