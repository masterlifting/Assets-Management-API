using IM.Service.Shared.RabbitMq;
using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.DataAccess.Comparators;
using IM.Service.Recommendations.Domain.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Shared.Models.RabbitMq.Api;

namespace IM.Service.Recommendations.Services.RabbitMq.Sync.Processes;

public class AssetProcess : IRabbitProcess
{
    private const string serviceName = "Asset synchronization";
    private readonly Repository<Asset> assetRepo;
    public AssetProcess(Repository<Asset> assetRepo) => this.assetRepo = assetRepo;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => model switch
    {
        AssetMqDto dto => action switch
        {
            QueueActions.Create => assetRepo.CreateAsync(GetAsset(dto), serviceName),
            QueueActions.Update => assetRepo.UpdateAsync(new[] { dto.Id }, GetAsset(dto), serviceName),
            QueueActions.Delete => assetRepo.DeleteAsync(new[] { dto.Id }, serviceName),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => models switch
    {
        AssetMqDto[] dtos => action switch
        {
            QueueActions.Set => assetRepo.CreateUpdateDeleteAsync(GetAssets(dtos), new AssetComparer(), serviceName),
            QueueActions.Create => assetRepo.CreateRangeAsync(GetAssets(dtos), new AssetComparer(), serviceName),
            QueueActions.Update => assetRepo.UpdateRangeAsync(GetAssets(dtos), serviceName),
            QueueActions.Delete => assetRepo.DeleteRangeAsync(GetAssets(dtos), serviceName),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };

    private static Asset GetAsset(AssetMqDto model) =>
        new()
        {
            Id = model.Id,
            TypeId = model.TypeId,
            CountryId = model.CountryId,
            Name = model.Name
        };
    private static Asset[] GetAssets(IEnumerable<AssetMqDto> models) =>
        models
            .Select(x => new Asset
            {
                Id = x.Id,
                TypeId = x.TypeId,
                CountryId = x.CountryId,
                Name = x.Name
            })
            .ToArray();
}