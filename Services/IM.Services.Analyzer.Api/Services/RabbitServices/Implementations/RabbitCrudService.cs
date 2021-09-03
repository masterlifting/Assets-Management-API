using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.RabbitServices;
using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;

using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        private readonly EntityRepository<Ticker, AnalyzerContext> repository;
        public RabbitCrudService(EntityRepository<Ticker, AnalyzerContext> repository)
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
                    ? await repository.DeleteAsync(data, data)
                    : RabbitHelper.TrySerialize(data, out AnalyzerTickerDto? ticker) && await GetActionAsync(repository!, action, new Ticker(ticker!), ticker!.Name);
        }

        private static async Task<bool> GetActionAsync<T>(EntityRepository<T, AnalyzerContext> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.create => await repository.CreateAsync(data, value),
            QueueActions.update => await repository.UpdateAsync(data, value),
            _ => true
        };
    }
}
