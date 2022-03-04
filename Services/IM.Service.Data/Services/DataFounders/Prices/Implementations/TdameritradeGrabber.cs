﻿using IM.Service.Common.Net;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Data.Clients;
using IM.Service.Data.Domain.DataAccess;
using IM.Service.Data.Domain.Entities;
using IM.Service.Data.Models.Services;

namespace IM.Service.Data.Services.DataFounders.Prices.Implementations;

public class TdameritradeGrabber : IDataGrabber
{
    private readonly Repository<Price> repository;
    private readonly ILogger<PriceGrabber> logger;
    private readonly TdAmeritradeClient client;

    public TdameritradeGrabber(Repository<Price> repository, ILogger<PriceGrabber> logger, TdAmeritradeClient client)
    {
        this.repository = repository;
        this.logger = logger;
        this.client = client;
    }

    public async Task GrabCurrentDataAsync(DataConfigModel config)
    {
        try
        {
            if (config.SourceValue is null)
                throw new ArgumentNullException(config.CompanyId);

            var data = await client.GetCurrentPricesAsync(new[] { config.CompanyId });

            var result = PriceHelper.PriceMapper.Map(data);

            result = result.Where(x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow)).ToArray();

            await repository.CreateUpdateAsync(result, new CompanyDateComparer<Price>(), "Tdameritrade current prices");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(TdameritradeGrabber) + '.' + nameof(GrabCurrentDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }
    public async Task GrabCurrentDataAsync(IEnumerable<DataConfigModel> configs)
    {
        try
        {
            var data = await client.GetCurrentPricesAsync(configs.Select(x => x.CompanyId));

            var result = PriceHelper.PriceMapper.Map(data);

            result = result.Where(x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow)).ToArray();

            await repository.CreateUpdateAsync(result, new CompanyDateComparer<Price>(), "Tdameritrade current prices");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(TdameritradeGrabber) + '.' + nameof(GrabCurrentDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }

    public async Task GrabHistoryDataAsync(DataConfigModel config)
    {
        try
        {
            if (config.SourceValue is null)
                throw new ArgumentNullException(config.CompanyId);

            var data = await client.GetHistoryPricesAsync(config.CompanyId);

            var result = PriceHelper.PriceMapper.Map(data);

            await repository.CreateAsync(result, new CompanyDateComparer<Price>(), "Tdameritrade history prices");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(TdameritradeGrabber) + '.' + nameof(GrabHistoryDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }
    public async Task GrabHistoryDataAsync(IEnumerable<DataConfigModel> configs)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(100));

        foreach (var config in configs)
            if (await timer.WaitForNextTickAsync())
                await GrabHistoryDataAsync(config);
    }
}