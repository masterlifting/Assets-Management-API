using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Data.DataAccess;
using IM.Service.Company.Data.DataAccess.Comparators;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace IM.Service.Company.Data.Services.MqServices.Implementations;

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
        var companyRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RepositorySet<DataAccess.Entities.Company>>();

        if (action == QueueActions.Delete)
            return (await companyRepository.DeleteAsync(data, data)).error is not null;

        if (!RabbitHelper.TrySerialize(data, out CompanyDto? dto))
            return false;

        var company = new DataAccess.Entities.Company
        {
            Id = dto!.Id,
            Name = dto.Name,
        };

        if (!await GetActionAsync(companyRepository, action, company, company.Name))
            return false;

        if (dto.Sources is null || !dto.Sources.Any())
            return true;

        var companySourceTypes = dto.Sources.Select(x => new CompanySourceType
        {
            CompanyId = dto.Id,
            SourceTypeId = x.Id,
            Value = x.Value
        });

        var cstRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RepositorySet<CompanySourceType>>();
        return await GetActionsAsync(cstRepository, action, companySourceTypes, new CompanySourceTypeComparer(), $"Sources for '{company.Name}'");
    }
    private static async Task<bool> GetActionAsync<T>(Repository<T, DatabaseContext> repository, QueueActions action, T data, string value) where T : class => action switch
    {
        QueueActions.Create => (await repository.CreateAsync(data, value)).error is null,
        QueueActions.Update => (await repository.CreateUpdateAsync(data, value)).error is null,
        _ => true
    };
    private static async Task<bool> GetActionsAsync<T>(Repository<T, DatabaseContext> repository, QueueActions action, IEnumerable<T> data, IEqualityComparer<T> comparer, string value) where T : class => action switch
    {
        QueueActions.Create => (await repository.CreateAsync(data, value)).error is null,
        QueueActions.Update => (await repository.CreateUpdateDeleteAsync(data, comparer, value)).error is null,
        _ => true
    };
}