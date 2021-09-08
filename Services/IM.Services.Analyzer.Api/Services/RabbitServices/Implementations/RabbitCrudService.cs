using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.RabbitServices;

using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.DataAccess.Repository;

using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        private readonly RepositorySet<Ticker> repository;
        public RabbitCrudService(RepositorySet<Ticker> repository)
        {
            this.repository = repository;
        }
        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
        {
            QueueEntities.ticker => await GetTickerResultAsync(action, data),
            _ => true
        };

        private async Task<bool> GetTickerResultAsync(QueueActions action, string data)
        {
            return action == QueueActions.delete
                    ? !(await repository.DeleteAsync(data, data)).Any()
                    : RabbitHelper.TrySerialize(data, out AnalyzerTickerDto? ticker) && await GetActionAsync(repository!, action, new Ticker(ticker!), ticker!.Name);
        }

        private static async Task<bool> GetActionAsync<T>(RepositorySet<T> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.create => !(await repository.CreateAsync(data, value)).errors.Any(),
            QueueActions.update => !(await repository.CreateUpdateAsync(data, value)).Any(),
            _ => true
        };
    }
}
