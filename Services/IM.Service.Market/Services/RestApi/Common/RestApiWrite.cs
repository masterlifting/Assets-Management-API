using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Services.RestApi.Mappers.Interfaces;
using IM.Service.Market.Settings;

using Microsoft.Extensions.Options;

namespace IM.Service.Market.Services.RestApi.Common;

public class RestApiWrite<TEntity, TPost> where TPost : class where TEntity : class, IDataIdentity, IPeriod
{
    private readonly Repository<TEntity> repository;
    private readonly Repository<CompanySource> companySourceRepo;
    private readonly IMapperWrite<TEntity, TPost> mapper;
    private readonly string rabbitConnectionString;

    private readonly Dictionary<string, (QueueEntities single, QueueEntities multiply)> sources;

    public RestApiWrite(IOptions<ServiceSettings> options, Repository<CompanySource> companySourceRepo, Repository<TEntity> repository, IMapperWrite<TEntity, TPost> mapper)
    {
        sources = new(StringComparer.OrdinalIgnoreCase)
        {
            { nameof(Price), (QueueEntities.Price, QueueEntities.Prices) },
            { nameof(Report), (QueueEntities.Report, QueueEntities.Reports) },
            { nameof(Split), (QueueEntities.Split, QueueEntities.Splits) },
            { nameof(Float), (QueueEntities.Float, QueueEntities.Floats) }
        };

        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        this.companySourceRepo = companySourceRepo;
        this.mapper = mapper;
        this.repository = repository;
    }

    public async Task<(string? error, TEntity? entity)> CreateAsync(string companyId, int sourceId, TPost model)
    {
        var entity = mapper.MapTo(companyId.ToUpperInvariant(), (byte)sourceId, model);

        var message = $"{typeof(TEntity).Name} of '{entity.CompanyId}' create";

        return await repository.CreateAsync(entity, message);
    }
    public async Task<(string? error, TEntity[]? entities)> CreateAsync(string companyId, int sourceId, IEnumerable<TPost> models, IEqualityComparer<TEntity> comparer)
    {
        var entities = mapper.MapTo(companyId.ToUpperInvariant(), (byte)sourceId, models);

        return await repository.CreateAsync(entities, comparer, $"Source count: {entities.Length}");
    }

    public async Task<(string? error, TEntity? entity)> UpdateAsync(TEntity id, TPost model)
    {
        var entity = mapper.MapTo(id, model);

        var info = $"{typeof(TEntity)} of '{entity.CompanyId}' update";

        return await repository.UpdateAsync(entity, info);
    }

    public async Task<(string? error, TEntity? entity)> DeleteAsync(TEntity id)
    {
        var info = $"{typeof(TEntity).Name} of '{id.CompanyId}' delete";

        return await repository.DeleteAsync(id, info);
    }


    public async Task<string> LoadAsync()
    {
        var entityName = typeof(TEntity).Name;

        var isSource = sources.ContainsKey(entityName);

        if (!isSource)
            return $"{entityName} for load not found";

        var companySources = await companySourceRepo.GetSampleAsync(x => x);

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, sources[entityName].multiply, QueueActions.Get, companySources);

        return $"Task for load data of {entityName} is starting...";
    }
    public async Task<string> LoadAsync(string companyId)
    {
        var entityName = typeof(TEntity).Name;

        var isSource = sources.ContainsKey(entityName);

        if (!isSource)
            return $"{entityName} for load not found";

        companyId = companyId.Trim().ToUpperInvariant();
        var companySources = await companySourceRepo.GetSampleAsync(x => x.CompanyId == companyId);

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, sources[entityName].multiply, QueueActions.Get, companySources);

        return $"Task for load data of {entityName}.{companyId}  is starting...";
    }
    public async Task<string> LoadAsync(byte sourceId)
    {
        var entityName = typeof(TEntity).Name;

        var isSource = sources.ContainsKey(entityName);

        if (!isSource)
            return $"{entityName} for load not found";

        var companySources = await companySourceRepo.GetSampleAsync(x => x.SourceId == sourceId);

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, sources[entityName].multiply, QueueActions.Get, companySources);

        return $"Task for load data of {entityName}.{sourceId}  is starting...";
    }
    public async Task<string> LoadAsync(string companyId, byte sourceId)
    {
        var entityName = typeof(TEntity).Name;

        var isSource = sources.ContainsKey(entityName);

        if (!isSource)
            return $"{entityName} for load not found";

        companyId = companyId.Trim().ToUpperInvariant();
        var companySource = await companySourceRepo
            .FindAsync(x =>
                x.CompanyId == companyId
                && x.SourceId == sourceId);

        if (companySource?.Value is null)
            return $"{entityName} source for load not found";

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, sources[entityName].single, QueueActions.Get, companySource);

        return $"Task for load data of {entityName}.{companyId}.{companySource.Value}  is starting...";
    }
}
