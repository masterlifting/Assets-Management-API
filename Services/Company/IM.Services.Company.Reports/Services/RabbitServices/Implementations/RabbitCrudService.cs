using CommonServices.Models.Dto.CompaniesReportsService;
using CommonServices.RabbitServices;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Services.Company.Reports.DataAccess.Entities;
using IM.Services.Company.Reports.DataAccess.Repository;

namespace IM.Services.Company.Reports.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        private readonly string rabbitConnectionString;
        private readonly RepositorySet<Ticker> repository;

        public RabbitCrudService(string rabbitConnectionString, RepositorySet<Ticker> repository)
        {
            this.rabbitConnectionString = rabbitConnectionString;
            this.repository = repository;
        }

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
        {
            QueueEntities.Ticker => await GetTickerResultAsync(action, data),
            _ => true
        };

        private async Task<bool> GetTickerResultAsync(QueueActions action, string data)
        {
            if (action == QueueActions.Delete)
                return !(await repository.DeleteAsync(data, data)).Any();

            if (!RabbitHelper.TrySerialize(data, out CompaniesReportsTickerDto ticker) && ticker is null)
                return false;


            if (await GetActionAsync(repository, action, new Ticker(ticker), ticker.Name))
            {
                var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Loader);
                publisher.PublishTask(QueueNames.CompaniesReports, QueueEntities.Report, QueueActions.Download, JsonSerializer.Serialize(ticker));
                return true;
            }

            return false;
        }
        private static async Task<bool> GetActionAsync<T>(RepositorySet<T> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.Create => !(await repository.CreateAsync(data, value)).errors.Any(),
            QueueActions.Update => !(await repository.CreateUpdateAsync(data, value)).Any(),
            _ => true
        };
    }
}
