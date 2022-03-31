using IM.Service.Common.Net;
using IM.Service.Common.Net.RabbitServices;
using System.Runtime.Serialization;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Interfaces;
using static IM.Service.Common.Net.Helper;

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

    public async Task SetAsync<T>(string data) where T : class, IDataIdentity
    {
        if (!RabbitHelper.TrySerialize(data, out T? entity))
            throw new SerializationException(typeof(T).Name);

        var coefficients = entity switch
        {
            Report _report => await GetAsync(_report),
            Price _price => await GetAsync(_price),
            Float _float => await GetAsync(_float),
            _ => throw new ArgumentOutOfRangeException($"{typeof(T).Name} not recognized")
        };

        var (error, _) = await coefficientRepo
            .CreateUpdateAsync(coefficients, new DataQuarterComparer<Coefficient>(), nameof(SetAsync));

        if (error is not null)
            throw new Exception(error);
    }
    public async Task SetRangeAsync<T>(string data) where T : class, IDataIdentity
    {
        if (!RabbitHelper.TrySerialize(data, out T[]? entities))
            throw new SerializationException(typeof(T).Name);

        var coefficients = entities switch
        {
            Report[] reports => await GetAsync(reports),
            Price[] prices => await GetAsync(prices),
            Float[] floats => await GetAsync(floats),
            _ => throw new ArgumentOutOfRangeException($"{typeof(T).Name} not recognized")
        };

        var (error, _) = await coefficientRepo
            .CreateUpdateAsync(coefficients, new DataQuarterComparer<Coefficient>(), nameof(SetRangeAsync));

        if (error is not null)
            throw new Exception(error);
    }


    private async Task<Coefficient[]> GetAsync(Report report, Float[]? floats = null, Price[]? prices = null)
    {
        var lastDate = new DateOnly(report.Year, QuarterHelper.GetLastMonth(report.Quarter), 28);

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

        if (!_floats.Any())
            return Array.Empty<Coefficient>();

        var coefficients = new List<Coefficient>(1)
        {
            await GetAsync(report, _floats[^1], prices)
        };

        return coefficients.ToArray();
    }
    private async Task<Coefficient[]> GetAsync(Float _float, Float[]? floats = null, Report[]? reports = null, Price[]? prices = null)
    {
        var firstDate = _float.Date;
        DateOnly? lastDate = null;

        /* При изменении количества ценных бумаг (входящего параметра) необходимо выяснить -
         какое количество отчетов было выпущено после этого изменения и были ли еще 
        изменения ценных бумаг после этого.
        Так будет выяснен период времени, на который влияет этот показатель*/
        var _floats = floats is null
            ? await floatRepo
                .GetSampleOrderedAsync(x =>
                    x.CompanyId == _float.CompanyId
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date > firstDate,
                    orderBy => orderBy.Date)
            : floats
                .Where(x =>
                    x.CompanyId == _float.CompanyId
                    //&& x.SourceId == // при необходимости можно указать источник данных
                    && x.Date > firstDate)
                .OrderBy(x => x.Date)
                .ToArray();

        if (_floats.Any())
            lastDate = _floats[^1].Date;

        /*Далее за этот период необходимо получить все отчеты, по которым в последствии
         будут пересчитаны коефициенты с учетом входящего параметра*/
        var _reports = lastDate.HasValue
            ? reports is null
                ? await reportRepo
                    .GetSampleAsync(x =>
                        x.CompanyId == _float.CompanyId
                        && (x.Year > firstDate.Year
                            || x.Year == firstDate.Year && x.Quarter >= QuarterHelper.GetQuarter(firstDate.Month))
                        && (x.Year < lastDate.Value.Year
                            || x.Year == lastDate.Value.Year &&
                            x.Quarter < QuarterHelper.GetQuarter(lastDate.Value.Month)))
                : reports
                    .Where(x =>
                        x.CompanyId == _float.CompanyId
                        && (x.Year > firstDate.Year
                            || x.Year == firstDate.Year && x.Quarter >= QuarterHelper.GetQuarter(firstDate.Month))
                        && (x.Year < lastDate.Value.Year
                            || x.Year == lastDate.Value.Year &&
                            x.Quarter < QuarterHelper.GetQuarter(lastDate.Value.Month)))
                    .ToArray()
            : reports is null
                ? await reportRepo
                    .GetSampleAsync(x =>
                        x.CompanyId == _float.CompanyId
                        && (x.Year > firstDate.Year
                            || x.Year == firstDate.Year && x.Quarter >= QuarterHelper.GetQuarter(firstDate.Month)))
                : reports.Where(x =>
                        x.CompanyId == _float.CompanyId
                        && (x.Year > firstDate.Year
                            || x.Year == firstDate.Year && x.Quarter >= QuarterHelper.GetQuarter(firstDate.Month)))
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

        var quarter = QuarterHelper.GetQuarter(price.Date.Month);
        var lastMonth = QuarterHelper.GetLastMonth(quarter);
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

    private async Task<Coefficient[]> GetAsync(Report[] reports)
    {
        reports = reports.OrderBy(x => x.Year).ThenBy(x => x.Quarter).ToArray();

        var companyIds = reports.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var currencyIds = reports.GroupBy(x => x.CurrencyId).Select(x => x.Key).ToArray();

        var firstReport = reports[0];
        var lastReport = reports[^1];

        var firstDate = new DateOnly(firstReport.Year, QuarterHelper.GetFirstMonth(firstReport.Quarter), 1);
        var lastDate = new DateOnly(lastReport.Year, QuarterHelper.GetLastMonth(lastReport.Quarter), 28);

        var _floats = await floatRepo.GetSampleAsync(x =>
            companyIds.Contains(x.CompanyId)
            && x.Date >= firstDate
            && x.Date <= lastDate);
        var _prices = await priceRepo.GetSampleAsync(x =>
            companyIds.Contains(x.CompanyId)
            && currencyIds.Contains(x.CurrencyId)
            && x.Date >= firstDate
            && x.Date <= lastDate);

        var coefficients = new List<Coefficient>(reports.Length);

        foreach (var report in reports)
        {
            try
            {
                coefficients.AddRange(await GetAsync(report, _floats, _prices));
            }
            catch (Exception exception)
            {
                logger.LogWarning(LogEvents.Function, exception.Message);
            }
        }

        return coefficients.ToArray();
    }
    private async Task<Coefficient[]> GetAsync(Float[] floats)
    {
        floats = floats.OrderBy(x => x.Date).ToArray();

        var companyIds = floats.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();

        var firstDate = floats[0].Date;
        var lastDate = floats[^1].Date;

        var _floats = await floatRepo.GetSampleAsync(x =>
            companyIds.Contains(x.CompanyId)
            && x.Date >= firstDate
            && x.Date <= lastDate);

        var _reports = await reportRepo
            .GetSampleAsync(x =>
                companyIds.Contains(x.CompanyId)
                && (x.Year > firstDate.Year
                    || x.Year == firstDate.Year && x.Quarter >= QuarterHelper.GetQuarter(firstDate.Month))
                && (x.Year < lastDate.Year
                    || x.Year == lastDate.Year &&
                    x.Quarter < QuarterHelper.GetQuarter(lastDate.Month)));

        var currencyIds = _reports.GroupBy(x => x.CurrencyId).Select(x => x.Key).ToArray();

        var _prices = await priceRepo.GetSampleAsync(x =>
            companyIds.Contains(x.CompanyId)
            && currencyIds.Contains(x.CurrencyId)
            && x.Date >= firstDate
            && x.Date <= lastDate);

        var coefficients = new List<Coefficient>(_reports.Length);

        foreach (var _float in floats)
        {
            try
            {
                coefficients.AddRange(await GetAsync(_float, _floats, _reports, _prices));
            }
            catch (Exception exception)
            {
                logger.LogWarning(LogEvents.Function, exception.Message);
            }
        }

        return coefficients.ToArray();
    }
    private async Task<Coefficient[]> GetAsync(Price[] prices)
    {
        prices = prices.OrderBy(x => x.Date).ToArray();

        var companyIds = prices.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var currencyIds = prices.GroupBy(x => x.CurrencyId).Select(x => x.Key).ToArray();

        var firstDate = prices[0].Date;
        var lastDate = prices[^1].Date;

        var _reports = await reportRepo
            .GetSampleAsync(x =>
                companyIds.Contains(x.CompanyId)
                && currencyIds.Contains(x.CurrencyId)
                && (x.Year > firstDate.Year
                    || x.Year == firstDate.Year && x.Quarter >= QuarterHelper.GetQuarter(firstDate.Month))
                && (x.Year < lastDate.Year
                    || x.Year == lastDate.Year &&
                    x.Quarter < QuarterHelper.GetQuarter(lastDate.Month)));

        var _prices = await priceRepo.GetSampleAsync(x =>
            companyIds.Contains(x.CompanyId)
            && currencyIds.Contains(x.CurrencyId)
            && x.Date >= firstDate
            && x.Date <= lastDate);

        var coefficients = new List<Coefficient>(_reports.Length);

        foreach (var price in prices)
        {
            try
            {
                coefficients.AddRange(await GetAsync(price, _reports, _prices));
            }
            catch (Exception exception)
            {
                logger.LogWarning(LogEvents.Function, exception.Message);
            }
        }

        return coefficients.ToArray();
    }


    private async Task<Coefficient> GetAsync(Report report, Float _float, IEnumerable<Price>? prices = null)
    {
        var firstDate = new DateOnly(report.Year, QuarterHelper.GetFirstMonth(report.Quarter), 1);
        var lastDate = new DateOnly(report.Year, QuarterHelper.GetLastMonth(report.Quarter), 28);

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
            ? throw new ArithmeticException(nameof(_prices) + " not found")
            : Compute(report, _float, _prices[^1]);
    }
    private async Task<Coefficient> GetAsync(Report report, Price price, IEnumerable<Float>? floats = null)
    {
        var lastDate = new DateOnly(report.Year, QuarterHelper.GetLastMonth(report.Quarter), 28);

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
            ? throw new ArithmeticException(nameof(_floats) + " not found")
            : Compute(report, _floats[^1], price);
    }

    private static Coefficient Compute(Report report, Float _float, Price price)
    {
        var coefficient = new Coefficient
        {
            CompanyId = report.CompanyId,
            SourceId = report.SourceId,
            Year = report.Year,
            Quarter = report.Quarter
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