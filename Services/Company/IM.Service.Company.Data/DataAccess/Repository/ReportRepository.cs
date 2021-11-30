using System;
using System.Collections.Generic;
using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Common.Net;

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
    public Task GetCreateHandlerAsync(ref Report[] entities)
    {
        var comparer = new CompanyQuarterComparer<Report>();
        entities = entities.Distinct(comparer).ToArray();
        
        var exist = GetExist(entities);

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }

    public Task GetUpdateHandlerAsync(ref Report entity)
    {
        var ctxEntity = context.Reports.FindAsync(entity.CompanyId, entity.Year, entity.Quarter).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(Report)} data not found. ");

        ctxEntity.SourceType = entity.SourceType;
        ctxEntity.Multiplier = entity.Multiplier;
        ctxEntity.Turnover = entity.Turnover;
        ctxEntity.LongTermDebt = entity.LongTermDebt;
        ctxEntity.Asset = entity.Asset;
        ctxEntity.CashFlow = entity.CashFlow;
        ctxEntity.Obligation = entity.Obligation;
        ctxEntity.ProfitGross = entity.ProfitGross;
        ctxEntity.ProfitNet = entity.ProfitNet;
        ctxEntity.Revenue = entity.Revenue;
        ctxEntity.ShareCapital = entity.ShareCapital;

        entity = ctxEntity;

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

    public async Task<IList<Report>> GetDeleteHandlerAsync(IReadOnlyCollection<Report> entities)
    {
        var comparer = new CompanyQuarterComparer<Report>();
        var result = new List<Report>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.Reports.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public Task SetPostProcessAsync(Report entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        publisher.PublishTask(
            QueueNames.CompanyAnalyzer
            , QueueEntities.CompanyReport
            , QueueActions.Create
            , JsonSerializer.Serialize(new CompanyDateIdentityDto
            {
                CompanyId = entity.CompanyId,
                Date = new DateTime(entity.Year, CommonHelper.QarterHelper.GetFirstMonth(entity.Quarter),1)
            }));

        publisher.PublishTask(
            QueueNames.CompanyAnalyzer
            , QueueEntities.Coefficient
            , QueueActions.Create
            , JsonSerializer.Serialize(new CompanyDateIdentityDto
            {
                CompanyId = entity.CompanyId,
                Date = new DateTime(entity.Year, CommonHelper.QarterHelper.GetFirstMonth(entity.Quarter), 1)
            }));

        return Task.CompletedTask;
    }
    public Task SetPostProcessAsync(IReadOnlyCollection<Report> entities)
    {
        if (!entities.Any()) 
            return Task.CompletedTask;
        
        var report = entities.OrderBy(x => x.Year).ThenBy(x => x.Quarter).First();
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        publisher.PublishTask(
            QueueNames.CompanyAnalyzer
            , QueueEntities.CompanyReport
            , QueueActions.Create
            , JsonSerializer.Serialize(new CompanyDateIdentityDto
            {
                CompanyId = report.CompanyId,
                Date = new DateTime(report.Year, CommonHelper.QarterHelper.GetFirstMonth(report.Quarter), 1)
            }));

        publisher.PublishTask(
            QueueNames.CompanyAnalyzer
            , QueueEntities.Coefficient
            , QueueActions.Create
            , JsonSerializer.Serialize(new CompanyDateIdentityDto
            {
                CompanyId = report.CompanyId,
                Date = new DateTime(report.Year, CommonHelper.QarterHelper.GetFirstMonth(report.Quarter), 1)
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