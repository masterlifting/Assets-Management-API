using IM.Service.Common.Net.Models.Dto.Mq.Companies;
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
            QueueEntities.CompanyReport => await SetReportToCalculateAsync(data),
            QueueEntities.Price => await SetPriceToCalculateAsync(data),
            _ => true
        };
    private async Task<bool> SetReportToCalculateAsync(string data)
    {
        if (!RabbitHelper.TrySerialize(data, out ReportIdentityDto? dto))
            return false;

        var entity = new Report
        {
            CompanyId = dto!.CompanyId,
            Year = dto.Year,
            Quarter = dto.Quarter,
            StatusId = (byte)StatusType.Ready
        };

        var reportRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RepositorySet<Report>>();
        return (await reportRepository.CreateUpdateAsync(entity, $"Report for '{dto.CompanyId}'")).error is null;
    }
    private async Task<bool> SetPriceToCalculateAsync(string data)
    {
        if (!RabbitHelper.TrySerialize(data, out PriceIdentityDto? dto))
            return false;

        var entity = new Price
        {
            CompanyId = dto!.CompanyId,
            Date = dto.Date,
            StatusId = (byte)StatusType.Ready
        };

        var priceRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RepositorySet<Price>>();
        return (await priceRepository.CreateUpdateAsync(entity, $"Price for '{dto.CompanyId}'")).error is null;
    }
}