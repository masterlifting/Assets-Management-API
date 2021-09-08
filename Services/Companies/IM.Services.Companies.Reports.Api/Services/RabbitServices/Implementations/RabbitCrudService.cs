using CommonServices.Models.Dto.CompaniesReportsService;
using CommonServices.RabbitServices;

using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.DataAccess.Repository;

using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.RabbitServices.Implementations
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
            QueueEntities.ticker => await GetTickerResultAsync(action, data),
            _ => true
        };

        private async Task<bool> GetTickerResultAsync(QueueActions action, string data)
        {
            if (action == QueueActions.delete)
                return !(await repository.DeleteAsync(int.Parse(data), data)).Any();

            if (!RabbitHelper.TrySerialize(data, out CompaniesReportsTickerDto ticker) && ticker is null)
                return false;


            if (await GetActionAsync(repository, action, new Ticker(ticker), ticker.Name))
            {
                var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.loader);
                publisher.PublishTask(QueueNames.companiesreports, QueueEntities.report, QueueActions.download, JsonSerializer.Serialize(ticker));
                return true;
            }

            return false;
        }
        private static async Task<bool> GetActionAsync<T>(RepositorySet<T> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.create => !(await repository.CreateAsync(data, value)).errors.Any(),
            QueueActions.update => !(await repository.CreateUpdateAsync(data, value)).Any(),
            _ => true
        };
    }
}
