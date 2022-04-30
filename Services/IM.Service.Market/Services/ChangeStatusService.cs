using IM.Service.Common.Net.Helpers;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities.Interfaces;

using System.Runtime.Serialization;

namespace IM.Service.Market.Services;

public class ChangeStatusService<T> where T: class, IRating
{
    private readonly Repository<T> repository;
    protected ChangeStatusService(Repository<T> repository) => this.repository = repository;

    public async Task ChangeStatusAsync(string data, byte statusId)
    {
        if (!JsonHelper.TryDeserialize(data, out T? entity))
            throw new SerializationException(typeof(T).Name);

        entity!.StatusId = statusId;

        await repository.UpdateAsync(entity, $"Set status '{Enum.Parse<Enums.Statuses>(statusId.ToString())}'");
    }
    public async Task ChangeStatusRangeAsync(string data, byte statusId)
    {
        if (!JsonHelper.TryDeserialize(data, out T[]? entities))
            throw new SerializationException(typeof(T).Name);

        foreach (var entity in entities!)
            entity.StatusId = statusId;

        await repository.UpdateRangeAsync(entities, $"Set status '{Enum.Parse<Enums.Statuses>(statusId.ToString())}' for '{typeof(T).Name}s'");
    }
}