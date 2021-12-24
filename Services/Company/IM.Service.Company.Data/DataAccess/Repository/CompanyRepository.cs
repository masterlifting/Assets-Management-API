using IM.Service.Common.Net;
using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitServices.Configuration;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class CompanyRepository : RepositoryHandler<Entities.Company, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;

    public CompanyRepository(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<Entities.Company> GetUpdateHandlerAsync(object[] id, Entities.Company entity)
    {
        var dbEntity = await context.Companies.FindAsync(id);

        if (dbEntity is null)
            throw new SqlNullValueException(nameof(dbEntity));

        dbEntity.Name = entity.Name;
        dbEntity.CompanySourceTypes = entity.CompanySourceTypes;

        //await SetTestQueueAsync(dbEntity.Id);

        return dbEntity;
    }
    public override async Task<IEnumerable<Entities.Company>> GetUpdateRangeHandlerAsync(IEnumerable<Entities.Company> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => x.Id,
                y => y.Id,
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Name = New.Name;
            Old.CompanySourceTypes = New.CompanySourceTypes;
        }

        return result.Select(x => x.Old);
    }
    public override async Task<IEnumerable<Entities.Company>> GetDeleteRangeHandlerAsync(IEnumerable<Entities.Company> entities)
    {
        var comparer = new CompanyComparer<Entities.Company>();
        var ctxEntities = await context.Companies.ToArrayAsync();
        return ctxEntities.Except(entities, comparer);
    }

    public override IQueryable<Entities.Company> GetExist(IEnumerable<Entities.Company> entities)
    {
        var existData = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key)
            .ToArray();

        return context.Companies.Where(x => existData.Contains(x.Id));
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