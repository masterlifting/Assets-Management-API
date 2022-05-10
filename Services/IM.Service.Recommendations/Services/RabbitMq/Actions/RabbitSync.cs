using System.Runtime.Serialization;
using System.Threading.Tasks;

using IM.Service.Common.Net.Helpers;
using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Models.Api.Http;

using Microsoft.Extensions.DependencyInjection;

namespace IM.Service.Recommendations.Services.RabbitMq.Actions;

public class RabbitSync : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitSync(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
    {
        QueueEntities.Company => GetCompanyResultAsync(action, data),
        _ => Task.CompletedTask
    };
    private Task GetCompanyResultAsync(QueueActions action, string data)
    {
        var companyRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Company>>();
        if (action == QueueActions.Delete)
            return companyRepository.DeleteAsync(new[] { data }, data);

        if (!JsonHelper.TryDeserialize(data, out CompanyDto? dto))
            throw new SerializationException(nameof(CompanyDto));

        var company = new Company
        {
            Id = dto!.Id,
            Name = dto.Name
        };

        return GetActionAsync(companyRepository, action, new[] { company.Id }, company, company.Name);
    }
    private static Task GetActionAsync<T>(Repository<T, DatabaseContext> repository, QueueActions action, object[] id, T entity, string info) where T : class => action switch
    {
        QueueActions.Create => repository.CreateAsync(entity, info),
        QueueActions.Update => repository.CreateUpdateAsync(id, entity, info),
        _ => Task.CompletedTask
    };
}