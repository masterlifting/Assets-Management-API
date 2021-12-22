using IM.Service.Common.Net;
using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class ReportRepository : RepositoryHandler<Report, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public ReportRepository(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<Report>> GetUpdateRangeHandlerAsync(IEnumerable<Report> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.CompanyId, x.Year, x.Quarter),
                y => (y.CompanyId, y.Year, y.Quarter),
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

        return result.Select(x => x.Old);
    }
    public override async Task<IEnumerable<Report>> GetDeleteRangeHandlerAsync(IEnumerable<Report> entities)
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

    public override Task SetPostProcessAsync(Report entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        publisher.PublishTask(
            QueueNames.CompanyAnalyzer
            , QueueEntities.CompanyReport
            , QueueActions.Create
            , JsonSerializer.Serialize(new CompanyDateIdentityDto
            {
                CompanyId = entity.CompanyId,
                Date = new DateTime(entity.Year, CommonHelper.QarterHelper.GetFirstMonth(entity.Quarter), 1)
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
    public override Task SetPostProcessAsync(IReadOnlyCollection<Report> entities)
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

    public override IQueryable<Report> GetExist(IEnumerable<Report> entities)
    {
        entities = entities.ToArray();
        var yearMin = entities.Min(x => x.Year);
        var yearMax = entities.Max(x => x.Year);

        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.Reports.Where(x => existData.Contains(x.CompanyId) && x.Year >= yearMin && x.Year <= yearMax);
    }
}