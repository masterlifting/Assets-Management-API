using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.RabbitServices;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;

namespace IM.Service.Company.Analyzer.Services.RabbitServices.Implementations
{
    public class RabbitCrudService : IRabbitActionService
    {
        private readonly RepositorySet<Ticker> repository;
        public RabbitCrudService(RepositorySet<Ticker> repository) => this.repository = repository;

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
        {
            QueueEntities.Ticker => await GetTickerResultAsync(action, data),
            _ => true
        };

        private async Task<bool> GetTickerResultAsync(QueueActions action, string data)
        {
            return action == QueueActions.Delete
                    ? !(await repository.DeleteAsync(data, data)).Any()
                    : RabbitHelper.TrySerialize(data, out AnalyzerTickerDto? ticker) && await GetActionAsync(repository!, action, new Ticker(ticker!), ticker!.Name);
        }

        private static async Task<bool> GetActionAsync<T>(RepositorySet<T> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.Create => !(await repository.CreateAsync(data, value)).errors.Any(),
            QueueActions.Update => !(await repository.CreateUpdateAsync(data, value)).Any(),
            _ => true
        };
    }
}
