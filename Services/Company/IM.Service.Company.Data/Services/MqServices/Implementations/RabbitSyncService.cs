using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.DataAccess.Comparators;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;

using Microsoft.Extensions.DependencyInjection;

using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.MqServices.Implementations;

public class RabbitSyncService : RabbitRepositoryHandler, IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitSyncService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
    {
        QueueEntities.Company => await GetCompanyResultAsync(action, data),
        QueueEntities.Companies => await GetCompaniesResultAsync(action, data),
        _ => true
    };

    private async Task<bool> GetCompanyResultAsync(QueueActions action, string data)
    {
        if (!RabbitHelper.TrySerialize(data, out CompanyDto? dto))
            return false;

        var company = new DataAccess.Entities.Company
        {
            Id = dto!.Id,
            Name = dto.Name
        };

        var companyActionIsSuccess = await GetRepositoryActionAsync(
            scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<DataAccess.Entities.Company>>(),
            action,
            new[] { company.Id },
            company);

        if (!companyActionIsSuccess)
            return false;

        if (dto.Sources is null || !dto.Sources.Any())
            return true;

        var companySourceTypes = dto.Sources.Select(x => new CompanySourceType
        {
            CompanyId = dto.Id,
            SourceTypeId = x.Id,
            Value = x.Value
        });
        return await GetRepositoryActionAsync(
            scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<CompanySourceType>>(),
            action,
            companySourceTypes, new CompanySourceTypeComparer());
    }
    private async Task<bool> GetCompaniesResultAsync(QueueActions action, string data)
    {
        if (!RabbitHelper.TrySerialize(data, out CompanyDto[]? dtos))
            return false;

        var companies = dtos!.Select(x => new DataAccess.Entities.Company
        {
            Id = x.Id,
            Name = x.Name
        });

        var companyActionIsSuccess = await GetRepositoryActionAsync(
            scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<DataAccess.Entities.Company>>(),
            action,
            companies,
            new CompanyComparer<DataAccess.Entities.Company>());

        if (!companyActionIsSuccess)
            return false;

        var sources = dtos!
            .Where(x => x.Sources is not null && x.Sources.Any())
            .SelectMany(x => x.Sources!
                .Select(y => new CompanySourceType
                {
                    CompanyId = x.Id,
                    SourceTypeId = y.Id,
                    Value = y.Value
                }))
            .ToArray();

        if (!sources.Any())
            return true;

        return await GetRepositoryActionAsync(
            scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<CompanySourceType>>(),
            action,
            sources,
            new CompanySourceTypeComparer());
    }
}