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

namespace IM.Service.Company.Data.Services.MqServices.Implementations;

public class RabbitSyncService : IRabbitActionService
{
    private readonly RepositorySet<DataAccess.Entities.Company> companyRepository;
    private readonly RepositorySet<CompanySourceType> cstRepository;

    public RabbitSyncService(
        RepositorySet<DataAccess.Entities.Company> companyRepository,
        RepositorySet<CompanySourceType> cstRepository)
    {
        this.companyRepository = companyRepository;
        this.cstRepository = cstRepository;
    }

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
    {
        QueueEntities.Company => await GetCompanyResultAsync(action, data),
        _ => true
    };
   
    private async Task<bool> GetCompanyResultAsync(QueueActions action, string data)
    {
        if (action == QueueActions.Delete)
            return (await companyRepository.DeleteAsync(data, data)).error is not null;

        if (!RabbitHelper.TrySerialize(data, out CompanyDto? dto))
            return false;

        var company = new DataAccess.Entities.Company
        {
            Id = dto!.Id,
            Name = dto.Name,
        };

        var (error, companyResult) = await GetActionAsync(companyRepository, action, company, company.Name);

        if (error is not null)
            return false;

        if (dto.Sources is null || !dto.Sources.Any())
            return false;

        var companySourceTypes = dto.Sources.Select(x => new CompanySourceType
        {
            CompanyId = dto.Id,
            SourceTypeId = x.Id,
            Value = x.Value
        });

        var companySourceTypeResult = await GetActionsAsync(cstRepository, action, companySourceTypes, new CompanySourceTypeComparer(), $"sources for {companyResult!.Name}");

        return companySourceTypeResult.error is null;
    }
    private static async Task<(string? error, T? result)> GetActionAsync<T>(Repository<T, DatabaseContext> repository, QueueActions action, T data, string value) where T : class => action switch
    {
        QueueActions.Create => await repository.CreateAsync(data, value),
        QueueActions.Update => await repository.CreateUpdateAsync(data, value),
        _ => ("action not found", null)
    };
    private static async Task<(string? error, T[]? result)> GetActionsAsync<T>(Repository<T, DatabaseContext> repository, QueueActions action, IEnumerable<T> data, IEqualityComparer<T> comparer, string value) where T : class => action switch
    {
        QueueActions.Create => await repository.CreateAsync(data, comparer, value),
        QueueActions.Update => await repository.CreateUpdateAsync(data, comparer, value),
        _ => ("action not found", null)
    };
}