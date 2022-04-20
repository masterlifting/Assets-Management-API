using IM.Service.Common.Net;
using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Models.Clients;

using System.Globalization;
using static IM.Service.Common.Net.Enums;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.DataLoaders.Prices.Implementations;

public class MoexGrabber : IDataGrabber
{
    private readonly Repository<Price> repository;
    private readonly ILogger<DataLoader<Price>> logger;
    private readonly MoexClient client;

    public MoexGrabber(Repository<Price> repository, ILogger<DataLoader<Price>> logger, MoexClient client)
    {
        this.repository = repository;
        this.logger = logger;
        this.client = client;
    }

    public async Task GetCurrentDataAsync(CompanySource companySource)
    {
        try
        {
            if (companySource.Value is null)
                throw new ArgumentNullException(companySource.CompanyId);

            var data = await client.GetCurrentPricesAsync();

            var result = Map(data, new[] { companySource.CompanyId });

            result = result.Where(x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow)).ToArray();

            await repository.CreateUpdateAsync(result, new DataDateComparer<Price>(), "Moex current prices");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(MoexGrabber) + '.' + nameof(GetCurrentDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }
    public async Task GetCurrentDataAsync(IEnumerable<CompanySource> companySources)
    {
        try
        {
            var data = await client.GetCurrentPricesAsync();

            var result = Map(data, companySources.Select(x => x.CompanyId));

            result = result.Where(x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow)).ToArray();

            await repository.CreateUpdateAsync(result, new DataDateComparer<Price>(), "Moex current prices");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(MoexGrabber) + '.' + nameof(GetCurrentDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }

    public async Task GetHistoryDataAsync(CompanySource companySource)
    {
        try
        {
            if (companySource.Value is null)
                throw new ArgumentNullException(companySource.CompanyId);

            var data = await client.GetHistoryPricesAsync(companySource.CompanyId, DateTime.UtcNow.AddYears(-1));

            var result = Map(data);

            await repository.CreateAsync(result, new DataDateComparer<Price>(), "Moex history prices");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(MoexGrabber) + '.' + nameof(GetHistoryDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }
    public async Task GetHistoryDataAsync(IEnumerable<CompanySource> companySources)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(100));

        foreach (var item in companySources)
            if (await timer.WaitForNextTickAsync())
                await GetHistoryDataAsync(item);
    }

    private static Price[] Map(MoexCurrentPriceResultModel clientResult, IEnumerable<string> tickers)
    {
        var clientData = clientResult.Data.Marketdata.Data;

        var prepareData = clientData.Select(x => new
        {
            ticker = x[0].ToString(),
            date = x[48],
            price = x[12]
        })
        .Where(x => x.ticker != null);

        var tickersData = prepareData.Join(tickers, x => x.ticker, y => y, (x, y) => new
        {
            Ticker = y,
            Date = x.date.ToString(),
            Price = x.price.ToString()
        }).ToArray();

        var result = new Price[tickersData.Length];

        for (var i = 0; i < tickersData.Length; i++)
            if (DateOnly.TryParse(tickersData[i].Date, out var date)
                && decimal.TryParse(tickersData[i].Price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price))
                result[i] = new()
                {
                    CompanyId = tickersData[i].Ticker,
                    Date = date,
                    Value = price,
                    CurrencyId = (byte)Currencies.Rub,
                    SourceId = (byte)Sources.Moex,
                    StatusId = (byte)Statuses.New,
                };

        return result;
    }
    private static Price[] Map(MoexHistoryPriceResultModel clientResult)
    {
        var (moexHistoryPriceData, ticker) = clientResult;

        var clientData = moexHistoryPriceData.History.Data;

        var result = new List<Price>();

        foreach (var data in clientData)
        {
            var priceObject = data?[8];
            var dateObject = data?[1];

            if ((DateOnly.TryParse(dateObject?.ToString(), out var date)
                 && decimal.TryParse(priceObject?.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price)))
                result.Add(new()
                {
                    CompanyId = ticker,
                    Date = date,
                    Value = price,
                    CurrencyId = (byte)Currencies.Rub,
                    SourceId = (byte)Sources.Moex,
                    StatusId = (byte)Statuses.New
                });
        }

        return result.ToArray();
    }
}