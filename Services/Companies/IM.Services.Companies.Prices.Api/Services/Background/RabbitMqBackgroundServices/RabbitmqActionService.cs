using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.DataAccess.Repository;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.Background.RabbitMqBackgroundServices
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
        private static async Task<bool> GetTickerCrudResultAsync(string data, string actionName, IServiceScope scope)
        {
            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<Ticker>>();
            return repository is not null && actionName.Equals("delete", StringComparison.OrdinalIgnoreCase)
                ? await repository.DeleteAsync(data, data)
                : repository!.TrySerialize(data, out Ticker ticker)
                    && ticker is not null
                    && await GetCrudActionAsync(actionName, repository, ticker, ticker.Name);
        }
        private static async Task<bool> GetCrudActionAsync<T>(string actionName, EntityRepository<T> repository, T entity, string value) where T : class => actionName switch
        {
            "create" => await repository.CreateAsync(entity, value),
            "update" => await repository.UpdateAsync(entity, value),
            _ => false
        };
    }
}
