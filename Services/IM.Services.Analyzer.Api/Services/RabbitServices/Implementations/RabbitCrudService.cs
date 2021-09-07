using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.RabbitServices;

using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.DataAccess.Repository;

using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        private readonly AnalyzerRepository<Ticker> repository;
        public RabbitCrudService(AnalyzerRepository<Ticker> repository)
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

        private static async Task<bool> GetActionAsync<T>(AnalyzerRepository<T> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.create => await repository.CreateAsync(data, value) is not null,
            QueueActions.update => await repository.CreateOrUpdateAsync(data, value),
            _ => true
        };
    }
}
