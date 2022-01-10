using IM.Service.Common.Net.RabbitServices;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService.Comparators;

namespace IM.Service.Company.Data.Services.MqServices.Implementations;

public class RabbitTransferService : RabbitRepositoryHandler, IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitTransferService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
        {
            QueueEntities.CompanyReports => await SetReportsAsync(action, data),
            _ => true
        };
    
    private async Task<bool> SetReportsAsync(QueueActions action, string data)
    {
        if (!RabbitHelper.TrySerialize(data, out Report[]? reports))
            return false;

        var repository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Report>>();

        return await GetRepositoryActionAsync(repository, action, reports!, new CompanyQuarterComparer<Report>());
    }
}