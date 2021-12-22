using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Analyzer.DataAccess;
using IM.Service.Company.Analyzer.DataAccess.Repository;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.Services.MqServices.Implementations;

public class RabbitSyncService : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitSyncService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
    {
        QueueEntities.Company => await GetCompanyResultAsync(action, data),
        _ => true
    };
    private async Task<bool> GetCompanyResultAsync(QueueActions action, string data)
    {
        var companyRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<DataAccess.Entities.Company>>();

        if (action == QueueActions.Delete)
            return (await companyRepository.DeleteAsync(new[] { data }, $"{nameof(GetCompanyResultAsync)}.{data}")).error is not null;

        if (!RabbitHelper.TrySerialize(data, out CompanyDto? dto))
            return false;

        var company = new DataAccess.Entities.Company
        {
            Id = dto!.Id,
            Name = dto.Name
        };

        return await GetActionAsync(companyRepository, action, new[] { company.Id }, company, $"{nameof(GetCompanyResultAsync)}.{company.Name}");
    }
    private static async Task<bool> GetActionAsync<T>(Repository<T, DatabaseContext> repository, QueueActions action, object[] id, T entity, string info) where T : class => action switch
    {
        QueueActions.Create => (await repository.CreateAsync(entity, info)).error is null,
        QueueActions.Update => (await repository.CreateUpdateAsync(id, entity, info)).error is null,
        _ => true
    };
}