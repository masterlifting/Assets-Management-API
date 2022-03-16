using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.MarketData.Domain.DataAccess;
using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Domain.Entities.Interfaces;
using IM.Service.MarketData.Services.RestApi.Mappers.Interfaces;
using IM.Service.MarketData.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.MarketData.Services.RestApi.Common;

public class RestMethodWrite<TEntity, TPost> where TPost : class where TEntity : class, IDataIdentity, IPeriod
{
    private readonly Repository<TEntity> repository;
    private readonly IMapperWrite<TEntity, TPost> mapper;
    private readonly string rabbitConnectionString;

    private readonly Dictionary<string, (QueueEntities single, QueueEntities multiply)> sources;

    protected RestMethodWrite(IOptions<ServiceSettings> options, Repository<TEntity> repository, IMapperWrite<TEntity, TPost> mapper)
    {
        sources = new(StringComparer.OrdinalIgnoreCase)
        {
            { nameof(Price), (QueueEntities.Price, QueueEntities.Prices) },
            { nameof(Report), (QueueEntities.Report, QueueEntities.Reports) },
            { nameof(Split), (QueueEntities.Split, QueueEntities.Splits) },
            { nameof(Float), (QueueEntities.Float, QueueEntities.Floats) }
        };
        
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        this.mapper = mapper;
        this.repository = repository;
    }

    public async Task<(string? error, TEntity? entity)> CreateAsync(TPost model)
    {
        var entity = mapper.MapTo(model);

        var message = $"{typeof(TEntity).Name} of '{entity.CompanyId}' create";

        return await repository.CreateAsync(entity, message);
    }


    public async Task<(string? error, TEntity[]? entities)> CreateAsync(IEnumerable<TPost> models, IEqualityComparer<TEntity> comparer)
    {
        var entities = mapper.MapTo(models);

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


    public string Load()
    {
        var entityName = typeof(TEntity).Name;

        var isSource = sources.ContainsKey(entityName);

        if (!isSource)
            return $"{entityName} for load not found";

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, sources[entityName].multiply, QueueActions.Get, DateTime.UtcNow.ToShortDateString());
        return $"Task for load data of {entityName} is starting...";
    }
    public string Load(string companyId)
    {
        var entityName = typeof(TEntity).Name;

        var isSource = sources.ContainsKey(entityName);

        if (!isSource)
            return $"{entityName} for load not found";

        companyId = companyId.Trim().ToUpperInvariant();
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, sources[entityName].single, QueueActions.Get, companyId);
        return $"Task for load data of {entityName}.{companyId}  is starting...";
    }
}
