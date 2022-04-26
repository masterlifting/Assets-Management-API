using IM.Service.Common.Net.Helpers;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Interfaces;

using Microsoft.EntityFrameworkCore;

using System.Runtime.Serialization;

using static IM.Service.Common.Net.Helpers.LogHelper;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.Calculations;

public class CoefficientService
{
    private readonly Repository<Coefficient> coefficientRepo;
    private readonly Repository<Report> reportRepo;
    private readonly Repository<Float> floatRepo;
    private readonly Repository<Price> priceRepo;
    private readonly ILogger<CoefficientService> logger;

    public CoefficientService(
        ILogger<CoefficientService> logger,
        Repository<Coefficient> coefficientRepo,
        Repository<Report> reportRepo,
        Repository<Float> floatRepo,
        Repository<Price> priceRepo)
    {
        this.coefficientRepo = coefficientRepo;
        this.reportRepo = reportRepo;
        this.floatRepo = floatRepo;
        this.priceRepo = priceRepo;
        this.logger = logger;
    }

    public async Task SetCoefficientAsync<T>(string data, QueueActions action) where T : class, IDataIdentity
    {
        if (!JsonHelper.TryDeserialize(data, out T? entity))
            throw new SerializationException(typeof(T).Name);

        var coefficients = entity switch
        {
            Report _report => await GetAsync(action, _report),
            Price _price => await GetAsync(action, _price),
            Float _float => await GetAsync(action, _float),
            _ => throw new ArgumentOutOfRangeException($"{typeof(T).Name} not recognized")
        };

       await coefficientRepo.CreateUpdateAsync(coefficients, new DataQuarterComparer<Coefficient>(), nameof(SetCoefficientAsync));
    }
    public async Task SetCoefficientRangeAsync<T>(string data, QueueActions action) where T : class, IDataIdentity
    {
        if (!JsonHelper.TryDeserialize(data, out T[]? entities))
            throw new SerializationException(typeof(T).Name);

        var coefficients = entities switch
        {
            Report[] reports => await GetAsync(action, reports),
            Price[] prices => await GetAsync(action, prices),
            Float[] floats => await GetAsync(action, floats),
            _ => throw new ArgumentOutOfRangeException($"{typeof(T).Name} not recognized")
        };

        await coefficientRepo.CreateUpdateAsync(coefficients, new DataQuarterComparer<Coefficient>(), nameof(SetCoefficientRangeAsync));
    }


    private async Task<Coefficient[]> GetAsync(QueueActions action, Report report)
    {
        if (action is QueueActions.Delete)
        {
            var deletedReport = report;
            var lastReport = await reportRepo
                .GetQuery(x =>
                    x.CompanyId == deletedReport.CompanyId
                    && x.CurrencyId == deletedReport.CurrencyId
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Year <= deletedReport.Year || x.Year == deletedReport.Year && x.Quarter < deletedReport.Quarter)
                .LastOrDefaultAsync();

            report = lastReport ?? throw new NullReferenceException($"Не найден {nameof(Report)} для расчета");
        }

        return await GetAsync(report);
    }
    private async Task<Coefficient[]> GetAsync(QueueActions action, Float _float)
    {
        if (action is QueueActions.Delete)
        {
            var deletedFloat = _float;
            var lastFloat = await floatRepo
                .GetQuery(x =>
                    x.CompanyId == deletedFloat.CompanyId
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date < deletedFloat.Date)
                .LastOrDefaultAsync();

            _float = lastFloat ?? throw new NullReferenceException($"Не найден {nameof(Float)} для расчета");
        }

        return await GetAsync(_float);
    }
    private async Task<Coefficient[]> GetAsync(QueueActions action, Price price)
    {
        if (action is QueueActions.Delete)
        {
            var deletedPrice = price;
            var lastPrice = await priceRepo
                .GetQuery(x =>
                    x.CompanyId == deletedPrice.CompanyId
                    && x.CurrencyId == deletedPrice.CurrencyId
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date < deletedPrice.Date)
                .LastOrDefaultAsync();

            price = lastPrice ?? throw new NullReferenceException($"Не найден {nameof(Price)} для расчета");
        }

        return await GetAsync(price);
    }

    private async Task<Coefficient[]> GetAsync(QueueActions action, Report[] reports)
    {
        if (action is QueueActions.Delete)
        {
            var deletedReports = reports;
            var deletedCompanyIds = reports.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
            var deletedCurrencyIds = reports.GroupBy(x => x.CurrencyId).Select(x => x.Key).ToArray();
            var deletedYearMin = deletedReports.Min(x => x.Year);
            var deletedYearMax = deletedReports.Max(x => x.Year);
            var deletedQuarterMax = deletedReports.MaxBy(x => x.Year)!.Quarter;
            var lastDbReports = await reportRepo
                .GetSampleAsync(x =>
                    deletedCompanyIds.Contains(x.CompanyId)
                    && deletedCurrencyIds.Contains(x.CurrencyId)
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Year >= deletedYearMin
                    && x.Year < deletedYearMax || x.Year == deletedYearMax && x.Quarter < deletedQuarterMax);

            var lastReports = lastDbReports
                .GroupBy(x => (x.CompanyId, x.SourceId))
                .Select(x => x.OrderBy(y => y.Year).ThenBy(y => y.Quarter).Last())
                .ToArray();

            reports = !lastReports.Any()
                ? throw new NullReferenceException($"Не найдены {nameof(Report)} для расчета")
                : lastReports;
        }

        reports = reports.OrderBy(x => x.Year).ThenBy(x => x.Quarter).ToArray();

        var coefficients = new List<Coefficient>(reports.Length);

        foreach (var report in reports)
        {
            try
            {
                coefficients.AddRange(await GetAsync(report));
            }
            catch (Exception exception)
            {
                logger.LogWarning(nameof(SetCoefficientAsync), report.CompanyId, exception.Message);
            }
        }

        return coefficients.ToArray();
    }
    private async Task<Coefficient[]> GetAsync(QueueActions action, Float[] floats)
    {
        if (action is QueueActions.Delete)
        {
            var deletedFloats = floats;
            var deletedCompanyIds = floats.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
            var deletedDateMin = deletedFloats.Min(x => x.Date);
            var deletedDateMax = deletedFloats.Max(x => x.Date);
            var lastDbFloats = await floatRepo
                .GetSampleAsync(x =>
                    deletedCompanyIds.Contains(x.CompanyId)
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date >= deletedDateMin
                    && x.Date < deletedDateMax);

            var lastFloats = lastDbFloats
                .GroupBy(x => (x.CompanyId, x.SourceId))
                .Select(x => x.OrderBy(y => y.Date).Last())
                .ToArray();

            floats = !lastFloats.Any()
                     ? throw new NullReferenceException($"Не найдены {nameof(Float)} для расчета")
                     : lastFloats;
        }

        floats = floats.OrderBy(x => x.Date).ToArray();

        var companyIds = floats.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();

        var firstDate = floats[0].Date;
        var lastDate = floats[^1].Date;

        var reports = await reportRepo
            .GetSampleAsync(x =>
                companyIds.Contains(x.CompanyId)
                && (x.Year > firstDate.Year
                    || x.Year == firstDate.Year && x.Quarter >= LogicHelper.QuarterHelper.GetQuarter(firstDate.Month))
                && (x.Year < lastDate.Year
                    || x.Year == lastDate.Year &&
                    x.Quarter < LogicHelper.QuarterHelper.GetQuarter(lastDate.Month)));

        var coefficients = new List<Coefficient>(reports.Length);

        foreach (var report in reports)
        {
            try
            {
                coefficients.AddRange(await GetAsync(report, floats));
            }
            catch (Exception exception)
            {
                logger.LogWarning(LogEvents.Function, exception.Message);
            }
        }

        return coefficients.ToArray();
    }
    private async Task<Coefficient[]> GetAsync(QueueActions action, Price[] prices)
    {
        if (action is QueueActions.Delete)
        {
            var deletedPrices = prices;
            var deletedCompanyIds = prices.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
            var deletedCurrencyIds = prices.GroupBy(x => x.CurrencyId).Select(x => x.Key).ToArray();
            var deletedDateMin = deletedPrices.Min(x => x.Date);
            var deletedDateMax = deletedPrices.Max(x => x.Date);
            var lastDbPrices = await priceRepo
                .GetSampleAsync(x =>
                    deletedCompanyIds.Contains(x.CompanyId)
                    && deletedCurrencyIds.Contains(x.CurrencyId)
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date >= deletedDateMin
                    && x.Date < deletedDateMax);

            var lastPrices = lastDbPrices
                .GroupBy(x => (x.CompanyId, x.SourceId))
                .Select(x => x.OrderBy(y => y.Date).Last())
                .ToArray();

            prices = !lastPrices.Any()
                ? throw new NullReferenceException($"Не найдены {nameof(Price)} для расчета")
                : lastPrices;
        }

        prices = prices.OrderBy(x => x.Date).ToArray();

        var companyIds = prices.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var currencyIds = prices.GroupBy(x => x.CurrencyId).Select(x => x.Key).ToArray();

        var firstDate = prices[0].Date;
        var lastDate = prices[^1].Date;

        var reports = await reportRepo
            .GetSampleAsync(x =>
                companyIds.Contains(x.CompanyId)
                && currencyIds.Contains(x.CurrencyId)
                && (x.Year > firstDate.Year
                    || x.Year == firstDate.Year && x.Quarter >= LogicHelper.QuarterHelper.GetQuarter(firstDate.Month))
                && (x.Year < lastDate.Year
                    || x.Year == lastDate.Year &&
                    x.Quarter < LogicHelper.QuarterHelper.GetQuarter(lastDate.Month)));

        var coefficients = new List<Coefficient>(reports.Length);

        foreach (var report in reports)
        {
            try
            {
                coefficients.AddRange(await GetAsync(report, null, prices));
            }
            catch (Exception exception)
            {
                logger.LogWarning(nameof(SetCoefficientAsync), report.CompanyId, exception.Message);
            }
        }

        return coefficients.ToArray();
    }


    private async Task<Coefficient[]> GetAsync(Report report, Float[]? floats = null, Price[]? prices = null)
    {
        var lastDate = new DateOnly(report.Year, LogicHelper.QuarterHelper.GetLastMonth(report.Quarter), 28);

        floats = floats is null
            ? await floatRepo
                .GetSampleOrderedAsync(x =>
                    x.CompanyId == report.CompanyId
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date <= lastDate,
                    orderBy => orderBy.Date)
            : floats
                .Where(x =>
                    x.CompanyId == report.CompanyId
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date <= lastDate)
                .OrderBy(x => x.Date)
                .ToArray();

        if (!floats.Any())
            floats = await floatRepo.GetSampleOrderedAsync(x =>
                    x.CompanyId == report.CompanyId,
                orderBy => orderBy.Date);

        return !floats.Any()
            ? throw new ArithmeticException($"floats for '{report.CompanyId}' with date less '{lastDate}' not found")
            : new[] { await GetAsync(report, floats[^1], prices) };
    }
    private async Task<Coefficient[]> GetAsync(Float _float, Report[]? reports = null, Price[]? prices = null)
    {
        var firstDate = _float.Date;
        DateOnly? lastDate = null;

        /* При изменении количества ценных бумаг (входящего параметра) необходимо выяснить -
         какое количество отчетов было выпущено после этого изменения и были ли еще 
        изменения ценных бумаг после этого.
        Так будет выяснен период времени, на который влияет этот показатель*/
        var floats = await floatRepo
            .GetSampleOrderedAsync(x =>
                    x.CompanyId == _float.CompanyId
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date > firstDate,
                orderBy => orderBy.Date);

        if (floats.Any())
            lastDate = floats[^1].Date;

        /*Далее за этот период необходимо получить все отчеты, по которым в последствии
         будут пересчитаны коефициенты с учетом входящего параметра*/
        var _reports = lastDate.HasValue
            ? reports is null
                ? await reportRepo
                    .GetSampleAsync(x =>
                        x.CompanyId == _float.CompanyId
                        && (x.Year > firstDate.Year
                            || x.Year == firstDate.Year && x.Quarter >= LogicHelper.QuarterHelper.GetQuarter(firstDate.Month))
                        && (x.Year < lastDate.Value.Year
                            || x.Year == lastDate.Value.Year &&
                            x.Quarter < LogicHelper.QuarterHelper.GetQuarter(lastDate.Value.Month)))
                : reports
                    .Where(x =>
                        x.CompanyId == _float.CompanyId
                        && (x.Year > firstDate.Year
                            || x.Year == firstDate.Year && x.Quarter >= LogicHelper.QuarterHelper.GetQuarter(firstDate.Month))
                        && (x.Year < lastDate.Value.Year
                            || x.Year == lastDate.Value.Year &&
                            x.Quarter < LogicHelper.QuarterHelper.GetQuarter(lastDate.Value.Month)))
                    .ToArray()
            : reports is null
                ? await reportRepo
                    .GetSampleAsync(x =>
                        x.CompanyId == _float.CompanyId
                        && (x.Year > firstDate.Year
                            || x.Year == firstDate.Year && x.Quarter >= LogicHelper.QuarterHelper.GetQuarter(firstDate.Month)))
                : reports.Where(x =>
                        x.CompanyId == _float.CompanyId
                        && (x.Year > firstDate.Year
                            || x.Year == firstDate.Year && x.Quarter >= LogicHelper.QuarterHelper.GetQuarter(firstDate.Month)))
                    .ToArray();

        if (!_reports.Any())
            return Array.Empty<Coefficient>();

        var coefficients = new List<Coefficient>(_reports.Length);

        foreach (var report in _reports)
            coefficients.Add(await GetAsync(report, _float, prices));

        return coefficients.ToArray();
    }
    private async Task<Coefficient[]> GetAsync(Price price, Report[]? reports = null, Price[]? prices = null)
    {
        var firstDate = price.Date;

        var quarter = LogicHelper.QuarterHelper.GetQuarter(price.Date.Month);
        var lastMonth = LogicHelper.QuarterHelper.GetLastMonth(quarter);
        var lastDate = new DateOnly(price.Date.Year, lastMonth, 28);

        /*Необходимо выяснить, является ли входящая цена последней в квартале*/
        var _prices = prices is null
            ? await priceRepo
                .GetSampleAsync(x =>
                    x.CompanyId == price.CompanyId
                    && x.SourceId == price.SourceId
                    && x.Date > firstDate
                    && x.Date <= lastDate)
            : prices
                .Where(x =>
                    x.CompanyId == price.CompanyId
                    && x.SourceId == price.SourceId
                    && x.Date > firstDate
                    && x.Date <= lastDate)
                .ToArray();

        /*если тут есть значения, то это говорит о том, что пришедшая цена не последняя в этом квартале.
         А значит по ней не имеет смысл пересчитывать коефициенты т.к.
        коефициент расчитывается по данным квартального отчета с учетом последней доступной цены в этом квартале.
        Соответственно уже имеется расчитаный коефициент с более актуальной ценой*/
        if (_prices.Any())
            return Array.Empty<Coefficient>();

        /*Тут выясняются все имеющиеся отчеты (полученые по разным источникам)
         за квартал, в котором меняется цена (входящий параметр),
         по данным которых необходимо будет пересчитать коэфициенты*/
        var _reports = reports is null
            ? await reportRepo
                .GetSampleAsync(x =>
                    x.CompanyId == price.CompanyId
                    && x.CurrencyId == price.CurrencyId
                    && x.Year == price.Date.Year
                    && x.Quarter == quarter)
            : reports
                .Where(x =>
                    x.CompanyId == price.CompanyId
                    && x.CurrencyId == price.CurrencyId
                    && x.Year == price.Date.Year
                    && x.Quarter == quarter)
                .ToArray();

        if (!_reports.Any())
            return Array.Empty<Coefficient>();

        var coefficients = new List<Coefficient>(_reports.Length);

        foreach (var report in _reports)
            coefficients.Add(await GetAsync(report, price));

        return coefficients.ToArray();
    }


    private async Task<Coefficient> GetAsync(Report report, Float _float, IEnumerable<Price>? prices = null)
    {
        var firstDate = new DateOnly(report.Year, LogicHelper.QuarterHelper.GetFirstMonth(report.Quarter), 1);
        var lastDate = new DateOnly(report.Year, LogicHelper.QuarterHelper.GetLastMonth(report.Quarter), 28);

        var _prices = prices is null
            ? await priceRepo
                .GetSampleOrderedAsync(x =>
                    x.CompanyId == report.CompanyId
                    && x.CurrencyId == report.CurrencyId
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date >= firstDate
                    && x.Date <= lastDate,
                    orderBy => orderBy.Date)
            : prices
                .Where(x =>
                    x.CompanyId == report.CompanyId
                    && x.CurrencyId == report.CurrencyId
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date >= firstDate
                    && x.Date <= lastDate)
                .OrderBy(x => x.Date)
                .ToArray();

        return !_prices.Any()
            ? throw new ArithmeticException($"prices for '{report.CompanyId}' with date betwen '{firstDate}' - '{lastDate}' not found")
            : Compute(report, _float, _prices[^1]);
    }
    private async Task<Coefficient> GetAsync(Report report, Price price, IEnumerable<Float>? floats = null)
    {
        var lastDate = new DateOnly(report.Year, LogicHelper.QuarterHelper.GetLastMonth(report.Quarter), 28);

        var _floats = floats is null
            ? await floatRepo
                .GetSampleOrderedAsync(x =>
                        x.CompanyId == report.CompanyId
                        //&& x.SourceId == // при необходимости можно указать источник данных
                        && x.Date <= lastDate,
                orderBy => orderBy.Date)
            : floats
                .Where(x =>
                    x.CompanyId == report.CompanyId
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date <= lastDate)
                .OrderBy(x => x.Date)
                .ToArray();

        return !_floats.Any()
            ? throw new ArithmeticException($"floats for '{report.CompanyId}' with date less '{lastDate}' not found")
            : Compute(report, _floats[^1], price);
    }


    private static Coefficient Compute(Report report, Float _float, Price price)
    {
        var coefficient = new Coefficient
        {
            CompanyId = report.CompanyId,
            SourceId = report.SourceId,
            Year = report.Year,
            Quarter = report.Quarter,
            StatusId = (byte)Statuses.Ready
        };

        if (report.Asset is not null)
        {
            coefficient.Roa = report.ProfitNet / report.Asset * 100;
            coefficient.DebtLoad = report.Obligation / report.Asset;

            if (report.Revenue is not null)
                coefficient.Profitability = (report.ProfitNet / report.Revenue + report.Revenue / report.Asset) * 0.5m;

            if (report.Obligation is not null)
                coefficient.Pb = price.Value * _float.Value / ((report.Asset - report.Obligation) * report.Multiplier);
        }

        if (report.ShareCapital is not null)
            coefficient.Roe = report.ProfitNet / report.ShareCapital * 100;

        coefficient.Eps = report.ProfitNet * report.Multiplier / _float.Value;

        if (coefficient.Eps is not null)
            coefficient.Pe = price.Value / coefficient.Eps;

        return coefficient;
    }
}