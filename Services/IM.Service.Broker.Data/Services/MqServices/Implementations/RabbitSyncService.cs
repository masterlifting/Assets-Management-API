using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Broker.Data.DataAccess.Repository;

using Microsoft.Extensions.DependencyInjection;

using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitServices.Configuration;

namespace IM.Service.Broker.Data.Services.MqServices.Implementations;

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

        return await GetRepositoryActionAsync(
            scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<DataAccess.Entities.Company>>(), 
            action, 
            new[] { company.Id }, 
            company);
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

        return await GetRepositoryActionAsync(
            scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<DataAccess.Entities.Company>>(), 
            action, 
            companies, 
            new CompanyComparer<DataAccess.Entities.Company>());
    }
}