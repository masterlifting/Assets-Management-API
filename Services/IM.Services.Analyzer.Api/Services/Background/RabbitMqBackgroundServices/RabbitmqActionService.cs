using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.DataAccess.Repository;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.Background.RabbitMqBackgroundServices
{
    public static class RabbitmqActionService
    {
        public static async Task<bool> GetCrudActionResultAsync(string data, string routingKey, IServiceScope scope)
        {
            var routeParts = routingKey.Split('.');

            return routeParts[2] switch
            {
                "ticker" => await GetTickerCrudResultAsync(data, routeParts[1], scope),
                _ => false
            };
        }
        private static async Task<bool> GetTickerCrudResultAsync(string data, string action, IServiceScope scope)
        {
            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<Ticker, string>>();
            return repository.TrySerialize(data, out Ticker? ticker)
                && ticker is not null
                && await GetCrudActionAsync(action, repository, ticker.Name, ticker, ticker.Name);
        }
        private static async Task<bool> GetCrudActionAsync<TEntity, TId>(string action, EntityRepository<TEntity, TId> repository, TId id, TEntity entity, string value) where TEntity : class => action switch
        {
            "create" => await repository.AddAsync(entity, value),
            //"update" => await repository.EditAsync(id, entity, value),
            "delete" => await repository.RemoveAsync(id, value),
            _ => false
        };
    }
}
