using System;
using IM.Service.Common.Net.RepositoryService;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Common.Net;
using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class CompanyRepository : IRepositoryHandler<Entities.Company>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;

    public CompanyRepository(IOptions<ServiceSettings> options, DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public Task GetCreateHandlerAsync(ref Entities.Company entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref Entities.Company[] entities)
    {
        var comparer = new CompanyComparer<Entities.Company>();
        entities = entities.Distinct(comparer).ToArray();

        var exist = GetExist(entities);

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }

    public Task GetUpdateHandlerAsync(ref Entities.Company entity)
    {
        var ctxEntity = context.Companies.FindAsync(entity.Id).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(Entities.Company)} data not found. ");

        ctxEntity.Name = entity.Name;
        ctxEntity.CompanySourceTypes = entity.CompanySourceTypes;

        entity = ctxEntity;

        SetTestQueueAsync(entity.Id).GetAwaiter().GetResult();

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref Entities.Company[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        var result = exist
            .Join(entities, x => x.Id, y => y.Id,
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Name = New.Name;
            Old.CompanySourceTypes = New.CompanySourceTypes;
        }

        entities = result.Select(x => x.Old).ToArray();

        return Task.CompletedTask;
    }

    public async Task<IList<Entities.Company>> GetDeleteHandlerAsync(IReadOnlyCollection<Entities.Company> entities)
    {
        var comparer = new CompanyComparer<Entities.Company>();
        var companies = await context.Companies.ToArrayAsync();
        return companies.Except(entities, comparer).ToArray();
    }

    public Task SetPostProcessAsync(Entities.Company entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(IReadOnlyCollection<Entities.Company> entities) => Task.CompletedTask;

    private IQueryable<Entities.Company> GetExist(IEnumerable<Entities.Company> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key.ToLowerInvariant())
            .ToArray();

        return context.Companies.Where(x => existData.Contains(x.Name.ToLowerInvariant()));
    }

    private async Task SetTestQueueAsync(string companyId)
    {
        var price = await context.Prices.Where(x => x.CompanyId == companyId).OrderBy(x => x.Date).FirstOrDefaultAsync();
        var report = await context.Reports.Where(x => x.CompanyId == companyId).OrderBy(x => x.Year).ThenBy(x => x.Quarter).FirstOrDefaultAsync();
        var volume = await context.StockVolumes.Where(x => x.CompanyId == companyId).OrderBy(x => x.Date).FirstOrDefaultAsync();
        var split = await context.StockSplits.Where(x => x.CompanyId == companyId).OrderBy(x => x.Date).FirstOrDefaultAsync();

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        if (price is not null)
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.Create
                , JsonSerializer.Serialize(new CompanyDateIdentityDto
                {
                    CompanyId = price.CompanyId,
                    Date = price.Date
                }));

        if (price is not null)
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Coefficient
                , QueueActions.Create
                , JsonSerializer.Serialize(new CompanyDateIdentityDto
                {
                    CompanyId = price.CompanyId,
                    Date = price.Date
                }));

        if (report is not null)
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.CompanyReport
                , QueueActions.Create
                , JsonSerializer.Serialize(new CompanyDateIdentityDto
                {
                    CompanyId = report.CompanyId,
                    Date = new DateTime(report.Year, CommonHelper.QarterHelper.GetFirstMonth(report.Quarter), 1)
                }));

        if (report is not null)
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Coefficient
                , QueueActions.Create
                , JsonSerializer.Serialize(new CompanyDateIdentityDto
                {
                    CompanyId = report.CompanyId,
                    Date = new DateTime(report.Year, CommonHelper.QarterHelper.GetFirstMonth(report.Quarter), 1)
                }));


        if (split is not null)
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.Create
                , JsonSerializer.Serialize(new CompanyDateIdentityDto
                {
                    CompanyId = split.CompanyId,
                    Date = split.Date
                }));

        if (volume is not null)
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Coefficient
                , QueueActions.Create
                , JsonSerializer.Serialize(new CompanyDateIdentityDto
                {
                    CompanyId = volume.CompanyId,
                    Date = volume.Date
                }));
    }
}