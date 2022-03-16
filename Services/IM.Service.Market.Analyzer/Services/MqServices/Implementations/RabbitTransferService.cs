using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Market.Analyzer.DataAccess.Comparators;
using IM.Service.Market.Analyzer.DataAccess.Entities;
using IM.Service.Market.Analyzer.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;
using static IM.Service.Market.Analyzer.Enums;

namespace IM.Service.Market.Analyzer.Services.MqServices.Implementations;

public class RabbitTransferService : RabbitRepositoryHandler, IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitTransferService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
        {
            QueueEntities.Report => await SetAnalyzedEntityAsync(EntityTypes.Report, action, data),
            QueueEntities.Reports => await SetAnalyzedEntitiesAsync(EntityTypes.Report, action, data),
            
            QueueEntities.Price => await SetAnalyzedEntityAsync(EntityTypes.Price, action, data),
            QueueEntities.Prices => await SetAnalyzedEntitiesAsync(EntityTypes.Price, action, data),
            
            QueueEntities.Coefficient => await SetAnalyzedEntityAsync(EntityTypes.Coefficient, action, data),
            QueueEntities.Coefficients => await SetAnalyzedEntitiesAsync(EntityTypes.Coefficient, action, data),
            _ => true
        };
    
    private async Task<bool> SetAnalyzedEntityAsync(EntityTypes entityType, QueueActions action, string data)
    {
        if (!RabbitHelper.TrySerialize(data, out CompanyDateIdentityDto? dto))
            return false;

        var entity = new AnalyzedEntity
        {
            CompanyId = dto!.CompanyId,
            Date = dto.Date,
            AnalyzedEntityTypeId = (byte)entityType,
            StatusId = (byte)Statuses.Ready
        };

        return await GetRepositoryActionAsync(
            scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<AnalyzedEntity>>(), 
            action, 
            new object[] {entity.CompanyId, entity.AnalyzedEntityTypeId, entity.Date}, 
            entity);
    }
    private async Task<bool> SetAnalyzedEntitiesAsync(EntityTypes entityType, QueueActions action, string data)
    {
        if (!RabbitHelper.TrySerialize(data, out CompanyDateIdentityDto[]? dtos))
            return false;

        var entities = dtos!.Select(x => new AnalyzedEntity
        {
            CompanyId = x.CompanyId,
            Date = x.Date,
            AnalyzedEntityTypeId = (byte)entityType,
            StatusId = (byte)Statuses.Ready
        });

        return await GetRepositoryActionAsync(
                scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<AnalyzedEntity>>(),
                action,
                entities,
                new AnalyzedEntityComparer());
    }
}