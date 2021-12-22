using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.MqServices.Implementations;

public class RabbitTransferService : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitTransferService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) =>
        action == QueueActions.Create && entity switch
        {
            QueueEntities.CompanyReport => await SetAnalyzedEntityAsync(EntityTypes.Report, data),
            QueueEntities.Price => await SetAnalyzedEntityAsync(EntityTypes.Price, data),
            QueueEntities.Coefficient => await SetAnalyzedEntityAsync(EntityTypes.Coefficient, data),
            _ => true
        };
    private async Task<bool> SetAnalyzedEntityAsync(EntityTypes entityType, string data)
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

        var reportRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<AnalyzedEntity>>();
        return (await reportRepository.CreateUpdateAsync(new object[] { entity.CompanyId, entity.AnalyzedEntityTypeId, entity.Date }, entity, $"{nameof(SetAnalyzedEntityAsync)}.{dto.CompanyId}")).error is null;
    }
}