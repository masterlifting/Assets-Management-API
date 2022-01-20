using IM.Service.Common.Net;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.Clients.Price;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Models.Data;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static IM.Service.Company.Data.Services.DataServices.Prices.PriceHelper;

namespace IM.Service.Company.Data.Services.DataServices.Prices.Implementations;

public class MoexGrabber : IDataGrabber
{
    private readonly Repository<Price> repository;
    private readonly ILogger<PriceGrabber> logger;
    private readonly MoexClient client;

    public MoexGrabber(Repository<Price> repository, ILogger<PriceGrabber> logger, MoexClient client)
    {
        this.repository = repository;
        this.logger = logger;
        this.client = client;
    }

    public async Task GrabCurrentDataAsync(string source, DataConfigModel config)
    {
        try
        {
            if (config.SourceValue is null)
                throw new ArgumentNullException(config.CompanyId);

            var data = await client.GetCurrentPricesAsync();

            var result = PriceMapper.Map(source, data, new[] { config.CompanyId });

            result = result.Where(x => x.Date.Date == DateTime.UtcNow.Date).ToArray();

            await repository.CreateUpdateAsync(result, new CompanyDateComparer<Price>(), "Moex current prices");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(MoexGrabber) + '.' + nameof(GrabCurrentDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }
    public async Task GrabCurrentDataAsync(string source, IEnumerable<DataConfigModel> configs)
    {
        try
        {
            var data = await client.GetCurrentPricesAsync();

            var result = PriceMapper.Map(source, data, configs.Select(x => x.CompanyId));

            result = result.Where(x => x.Date.Date == DateTime.UtcNow.Date).ToArray();

            await repository.CreateUpdateAsync(result, new CompanyDateComparer<Price>(), "Moex current prices");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(MoexGrabber) + '.' + nameof(GrabCurrentDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }

    public async Task GrabHistoryDataAsync(string source, DataConfigModel config)
    {
        try
        {
            if (config.SourceValue is null)
                throw new ArgumentNullException(config.CompanyId);

            var data = await client.GetHistoryPricesAsync(config.CompanyId, DateTime.UtcNow.AddYears(-1));

            var result = PriceMapper.Map(source, data);
            
            await repository.CreateAsync(result, new CompanyDateComparer<Price>(), "Moex history prices");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(MoexGrabber) + '.' + nameof(GrabHistoryDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }
    public async Task GrabHistoryDataAsync(string source, IEnumerable<DataConfigModel> configs)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(100));

        foreach (var item in configs)
            if (await timer.WaitForNextTickAsync())
                await GrabHistoryDataAsync(source, item);
    }
}