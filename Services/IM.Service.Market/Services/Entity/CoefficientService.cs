using IM.Service.Shared.Helpers;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Helpers;
using Microsoft.EntityFrameworkCore;

using static IM.Service.Shared.Helpers.LogicHelper;
using IM.Service.Shared.RabbitMq;

namespace IM.Service.Market.Services.Entity;

public sealed class CoefficientService : StatusChanger<Coefficient>
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
        Repository<Price> priceRepo) : base(coefficientRepo)
    {
        this.coefficientRepo = coefficientRepo;
        this.reportRepo = reportRepo;
        this.floatRepo = floatRepo;
        this.priceRepo = priceRepo;
        this.logger = logger;
    }
    public async Task SetCoefficientAsync(QueueActions action, Report report)
    {
        if (action is not QueueActions.Delete)
        {
            await coefficientRepo.CreateUpdateRangeAsync(await GetCoefficientAsync(report), new DataQuarterComparer<Coefficient>(), nameof(SetCoefficientAsync) + $": {report.CompanyId}");
            return;
        }

        await coefficientRepo.DeleteAsync(new object[] { report.CompanyId, report.SourceId, report.Year, report.Quarter }, nameof(SetCoefficientAsync) + $": {report.CompanyId}");
    }
    public async Task SetCoefficientAsync(QueueActions action, Float _float)
    {
        if (action is not QueueActions.Delete)
        {
            await coefficientRepo.CreateUpdateRangeAsync(await GetCoefficientAsync(_float), new DataQuarterComparer<Coefficient>(), nameof(SetCoefficientAsync) + $": {_float.CompanyId}");
            return;
        }

        var quarter = QuarterHelper.GetQuarter(_float.Date.Month);
        var months = QuarterHelper.GetMonths(quarter);
        var dateStart = new DateOnly(_float.Date.Year, months[0], 1);
        var dateEnd = new DateOnly(_float.Date.Year, months[2], 28);

        var deletedFloat = _float;

        var lastFloat = await floatRepo
            .Where(x =>
                x.CompanyId == deletedFloat.CompanyId
                && x.Date >= dateStart && x.Date <= dateEnd)
            .OrderBy(x => x.Date)
            .LastOrDefaultAsync();

        _float = lastFloat ?? throw new NullReferenceException($"Не найден {nameof(Float)} для расчета");

        await coefficientRepo.CreateUpdateRangeAsync(await GetCoefficientAsync(_float), new DataQuarterComparer<Coefficient>(), nameof(SetCoefficientAsync) + $": {_float.CompanyId}");
    }
    public async Task SetCoefficientAsync(QueueActions action, Price price)
    {
        if (action is not QueueActions.Delete)
        {
            await coefficientRepo.CreateUpdateRangeAsync(await GetCoefficientAsync(price), new DataQuarterComparer<Coefficient>(), nameof(SetCoefficientAsync) + $": {price.CompanyId}");
            return;
        }

        var quarter = QuarterHelper.GetQuarter(price.Date.Month);
        var months = QuarterHelper.GetMonths(quarter);
        var dateStart = new DateOnly(price.Date.Year, months[0], 1);
        var dateEnd = new DateOnly(price.Date.Year, months[2], 28);

        var deletedPrice = price;

        var lastPrice = await priceRepo
            .Where(x =>
                x.CompanyId == deletedPrice.CompanyId
                && x.Date >= dateStart && x.Date <= dateEnd)
            .OrderBy(x => x.Date)
            .LastOrDefaultAsync();

        price = lastPrice ?? throw new NullReferenceException($"Не найден {nameof(Price)} для расчета");

        await coefficientRepo.CreateUpdateRangeAsync(await GetCoefficientAsync(price), new DataQuarterComparer<Coefficient>(), nameof(SetCoefficientAsync) + $": {price.CompanyId}");
    }

    public async Task SetCoefficientAsync(QueueActions action, Report[] reports)
    {
        if (action is QueueActions.Delete)
        {
            var deletedCoefficients = reports
                .Select(x => new Coefficient
                {
                    CompanyId = x.CompanyId,
                    SourceId = x.SourceId,
                    Year = x.Year,
                    Quarter = x.Quarter
                })
                .ToArray();

            await coefficientRepo.DeleteRangeAsync(deletedCoefficients, nameof(SetCoefficientAsync) + $": CompanyIds: {string.Join(";", deletedCoefficients.Select(x => x.CompanyId).Distinct())}");
        }

        reports = reports.OrderBy(x => x.Year).ThenBy(x => x.Quarter).ToArray();

        var coefficients = new List<Coefficient>(reports.Length);

        foreach (var report in reports)
        {
            try
            {
                coefficients.AddRange(await GetCoefficientAsync(report));
            }
            catch (Exception exception)
            {
                logger.LogWarning(nameof(GetCoefficientAsync), report.CompanyId, exception.Message);
            }
        }

        await coefficientRepo.CreateUpdateRangeAsync(coefficients, new DataQuarterComparer<Coefficient>(), nameof(SetCoefficientAsync) + $": CompanyIds: {string.Join(";", coefficients.Select(x => x.CompanyId).Distinct())}");
    }
    public async Task SetCoefficientAsync(QueueActions action, Float[] floats)
    {
        if (action is QueueActions.Delete)
        {
            var deletedFloats = floats;
            var deletedCompanyIds = floats.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();

            var deletedDateMin = deletedFloats.Min(x => x.Date);
            var deletedDateMax = deletedFloats.Max(x => x.Date);

            var quarterMin = QuarterHelper.GetQuarter(deletedDateMin.Month);
            var quarterMax = QuarterHelper.GetQuarter(deletedDateMax.Month);

            var monthsMin = QuarterHelper.GetMonths(quarterMin);
            var monthsMax = QuarterHelper.GetMonths(quarterMax);

            var dateStart = new DateOnly(deletedDateMin.Year, monthsMin[0], 1);
            var dateEnd = new DateOnly(deletedDateMax.Year, monthsMax[2], 28);

            var lastDbFloats = await floatRepo
                .GetSampleAsync(x =>
                    deletedCompanyIds.Contains(x.CompanyId)
                    && x.Date >= dateStart && x.Date <= dateEnd);

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
                    || x.Year == firstDate.Year && x.Quarter >= QuarterHelper.GetQuarter(firstDate.Month))
                && (x.Year < lastDate.Year
                    || x.Year == lastDate.Year &&
                    x.Quarter < QuarterHelper.GetQuarter(lastDate.Month)));

        var coefficients = new List<Coefficient>(reports.Length);

        foreach (var report in reports)
        {
            try
            {
                coefficients.AddRange(await GetCoefficientAsync(report, floats));
            }
            catch (Exception exception)
            {
                logger.LogWarning(nameof(GetCoefficientAsync), report.CompanyId, exception.Message);
            }
        }

        await coefficientRepo.CreateUpdateRangeAsync(coefficients, new DataQuarterComparer<Coefficient>(), nameof(SetCoefficientAsync) + $": CompanyIds: {string.Join(";", coefficients.Select(x => x.CompanyId).Distinct())}");
    }
    public async Task SetCoefficientAsync(QueueActions action, Price[] prices)
    {
        if (action is QueueActions.Delete)
        {
            var deletedPrices = prices;
            var deletedCompanyIds = prices.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
            var deletedCurrencyIds = prices.GroupBy(x => x.CurrencyId).Select(x => x.Key).ToArray();
            var deletedDateMin = deletedPrices.Min(x => x.Date);
            var deletedDateMax = deletedPrices.Max(x => x.Date);

            var quarterMin = QuarterHelper.GetQuarter(deletedDateMin.Month);
            var quarterMax = QuarterHelper.GetQuarter(deletedDateMax.Month);

            var monthsMin = QuarterHelper.GetMonths(quarterMin);
            var monthsMax = QuarterHelper.GetMonths(quarterMax);

            var dateStart = new DateOnly(deletedDateMin.Year, monthsMin[0], 1);
            var dateEnd = new DateOnly(deletedDateMax.Year, monthsMax[2], 28);

            var lastDbPrices = await priceRepo
                .GetSampleAsync(x =>
                    deletedCompanyIds.Contains(x.CompanyId)
                    && deletedCurrencyIds.Contains(x.CurrencyId)
                    && x.Date >= dateStart && x.Date < dateEnd);

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
                    || x.Year == firstDate.Year && x.Quarter >= QuarterHelper.GetQuarter(firstDate.Month))
                && (x.Year < lastDate.Year
                    || x.Year == lastDate.Year &&
                    x.Quarter < QuarterHelper.GetQuarter(lastDate.Month)));

        var coefficients = new List<Coefficient>(reports.Length);

        foreach (var report in reports)
        {
            try
            {
                coefficients.AddRange(await GetCoefficientAsync(report, null, prices));
            }
            catch (Exception exception)
            {
                logger.LogWarning(nameof(GetCoefficientAsync), report.CompanyId, exception.Message);
            }
        }

        await coefficientRepo.CreateUpdateRangeAsync(coefficients, new DataQuarterComparer<Coefficient>(), nameof(SetCoefficientAsync) + $": CompanyIds: {string.Join(";", coefficients.Select(x => x.CompanyId).Distinct())}");
    }

    private async Task<Coefficient[]> GetCoefficientAsync(Report report, Float[]? floats = null, Price[]? prices = null)
    {
        var lastDate = new DateOnly(report.Year, QuarterHelper.GetLastMonth(report.Quarter), 28);

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
            : new[] { await GetCoefficientAsync(report, floats[^1], prices) };
    }
    private async Task<Coefficient[]> GetCoefficientAsync(Float _float, Report[]? reports = null, Price[]? prices = null)
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
            coefficients.Add(await GetCoefficientAsync(report, _float, prices));

        return coefficients.ToArray();
    }
    private async Task<Coefficient[]> GetCoefficientAsync(Price price, Report[]? reports = null, Price[]? prices = null)
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
            coefficients.Add(await GetCoefficientAsync(report, price));

        return coefficients.ToArray();
    }

    private async Task<Coefficient> GetCoefficientAsync(Report report, Float _float, IEnumerable<Price>? prices = null)
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
            ? throw new ArithmeticException($"prices for '{report.CompanyId}' with date betwen '{firstDate}' - '{lastDate}' not found")
            : Compute(report, _float, _prices[^1]);
    }
    private async Task<Coefficient> GetCoefficientAsync(Report report, Price price, IEnumerable<Float>? floats = null)
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
            StatusId = (byte)Enums.Statuses.Ready
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