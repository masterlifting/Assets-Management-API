using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.DataAccess.Repository;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.Background.RabbitMqBackgroundServices
{
    public static class RabbitmqActionService
    {
        public static async Task<bool> GetCrudActionResultAsync(string data, string routingKey, IServiceScope scope)
        {
            var routeParts = routingKey.Split('.');

            return routeParts[2] switch
            {
                "ticker" => await GetTickerCrudResultAsync(data, routeParts[1], scope),
                "reportsource" => await GetReportSourceCrudResultAsync(data, routeParts[1], scope),
                _ => false
            };
        }
        private static async Task<bool> GetTickerCrudResultAsync(string data, string action, IServiceScope scope)
        {
            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<Ticker, string>>();
            return action.Equals("delete", StringComparison.OrdinalIgnoreCase)
                ? await GetCrudActionAsync(action, repository, data, data)
                : repository.TrySerialize(data, out Ticker ticker) && await GetCrudActionAsync(action, repository, ticker.Name, ticker.Name, ticker);
        }
        private static async Task<bool> GetReportSourceCrudResultAsync(string data, string action, IServiceScope scope)
        {
            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<ReportSource, int>>();
            return action.Equals("delete", StringComparison.OrdinalIgnoreCase)
                ? await GetCrudActionAsync(action, repository, data, int.Parse(data))
                : repository.TrySerialize(data, out ReportSource source) && await GetCrudActionAsync(action, repository, source.Value, source.Id, source);
        }
        private static async Task<bool> GetCrudActionAsync<TEntity, TId>(string action, EntityRepository<TEntity, TId> repository, string value, TId id, TEntity entity = null) where TEntity : class
        {

            return action switch
            {
                "create" => await repository.CreateAsync(entity, value),
                "update" => await repository.UpdateAsync(id, entity, value),
                "delete" => await repository.DeleteAsync(id, value),
                _ => false
            };
        }
    }
}
