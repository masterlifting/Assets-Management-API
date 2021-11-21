using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using IM.Service.Common.Net.RabbitServices;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class ReportRepository : IRepositoryHandler<Report>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public ReportRepository(
        IOptions<ServiceSettings> options,
        DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public Task GetCreateHandlerAsync(ref Report entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref Report[] entities, IEqualityComparer<Report> comparer)
    {
        var exist = GetExist(entities);

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref Report entity)
    {
        var dbEntity = context.Reports.FindAsync(entity.CompanyId, entity.Year, entity.Quarter).GetAwaiter().GetResult();

        dbEntity.SourceType = entity.SourceType;
        dbEntity.Multiplier = entity.Multiplier;
        dbEntity.Turnover = entity.Turnover;
        dbEntity.LongTermDebt = entity.LongTermDebt;
        dbEntity.Asset = entity.Asset;
        dbEntity.CashFlow = entity.CashFlow;
        dbEntity.Obligation = entity.Obligation;
        dbEntity.ProfitGross = entity.ProfitGross;
        dbEntity.ProfitNet = entity.ProfitNet;
        dbEntity.Revenue = entity.Revenue;
        dbEntity.ShareCapital = entity.ShareCapital;

        entity = dbEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref Report[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        var result = exist
            .Join(entities, x => (x.CompanyId, x.Year, x.Quarter), y => (y.CompanyId, y.Year, y.Quarter),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.SourceType = New.SourceType;
            Old.Multiplier = New.Multiplier;
            Old.Turnover = New.Turnover;
            Old.LongTermDebt = New.LongTermDebt;
            Old.Asset = New.Asset;
            Old.CashFlow = New.CashFlow;
            Old.Obligation = New.Obligation;
            Old.ProfitGross = New.ProfitGross;
            Old.ProfitNet = New.ProfitNet;
            Old.Revenue = New.Revenue;
            Old.ShareCapital = New.ShareCapital;
        }

        entities = result.Select(x => x.Old).ToArray();

        return Task.CompletedTask;
    }

    public Task SetPostProcessAsync(Report entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        publisher.PublishTask(
            QueueNames.CompanyAnalyzer
            , QueueEntities.Report
            , QueueActions.Create
            , JsonSerializer.Serialize(new ReportIdentityDto
            {
                CompanyId = entity.CompanyId,
                Year = entity.Year,
                Quarter = entity.Quarter
            }));

        return Task.CompletedTask;
    }

    public Task SetPostProcessAsync(Report[] entities)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        foreach (var report in entities)
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Report
                , QueueActions.Create
                , JsonSerializer.Serialize(new ReportIdentityDto
                {
                    CompanyId = report.CompanyId,
                    Year = report.Year,
                    Quarter = report.Quarter
                }));

        return Task.CompletedTask;
    }

    private IQueryable<Report> GetExist(Report[] entities)
    {
        var yearMin = entities.Min(x => x.Year);
        var yearMax = entities.Max(x => x.Year);

        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.Reports.Where(x => existData.Contains(x.CompanyId) && x.Year >= yearMin && x.Year <= yearMax);
    }
}