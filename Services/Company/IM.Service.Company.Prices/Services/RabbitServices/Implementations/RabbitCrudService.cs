using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.RabbitServices;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.DataAccess.Repository;
// ReSharper disable InvertIf

namespace IM.Service.Company.Prices.Services.RabbitServices.Implementations
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

            if (!RabbitHelper.TrySerialize(data, out TickerPostDto? ticker))
                return false;

            if (await GetActionAsync(repository, action, new Ticker(ticker!), ticker!.Name))
            {
                var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Data);
                publisher.PublishTask(QueueNames.CompanyPrices, QueueEntities.Price, QueueActions.GetData, JsonSerializer.Serialize(ticker));

                return true;
            }

            return false;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static async Task<bool> GetActionAsync<T>(RepositorySet<T> repository, QueueActions action, T data, string value) where T : class => action switch
        {
            QueueActions.Create => !(await repository.CreateAsync(data, value)).errors.Any(),
            QueueActions.Update => !(await repository.CreateUpdateAsync(data, value)).Any(),
            _ => true
        };
    }
}
