using ExcelDataReader;

using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.DataAccess.Comparators;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Domain.Entities.Catalogs;
using IM.Service.Portfolio.Models.Api.Mq;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static IM.Service.Shared.Enums;
using static IM.Service.Shared.Helpers.LogHelper;
using static IM.Service.Portfolio.Enums;

// ReSharper disable RedundantJumpStatement
// ReSharper disable RedundantAssignment

namespace IM.Service.Portfolio.Services.Data.Reports.Implementations;

public sealed class BcsGrabber : IDataGrabber
{
    private const byte brokerId = (byte)Brokers.Bcs;

    private readonly Repository<Account> accountRepo;
    private readonly Repository<Derivative> derivativeRepo;
    private readonly Repository<Deal> dealRepo;
    private readonly Repository<Event> eventRepo;
    private readonly Repository<Report> reportRepo;
    private readonly ILogger logger;

    public BcsGrabber(
        Repository<Account> accountRepo,
        Repository<Derivative> derivativeRepo,
        Repository<Deal> dealRepo,
        Repository<Event> eventRepo,
        Repository<Report> reportRepo,
        ILogger logger)
    {
        this.reportRepo = reportRepo;
        this.dealRepo = dealRepo;
        this.accountRepo = accountRepo;
        this.derivativeRepo = derivativeRepo;
        this.logger = logger;
        this.eventRepo = eventRepo;
    }

    public async Task GetDataAsync(ReportFileDto file)
    {
        var dbAccounts = await accountRepo.GetSampleAsync(x => x.BrokerId == brokerId && x.UserId == file.UserId, x => ValueTuple.Create(x.Name, x.Id));
        var dbDerivatives = await derivativeRepo.GetSampleAsync(x => ValueTuple.Create(x.Id, x.Code));

        var accounts = dbAccounts
            .ToDictionary(x => x.Item1, x => x.Item2);
        var derivatives = dbDerivatives
            .GroupBy(x => x.Item1)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Item2).ToArray());

        try
        {
            var table = GetExcelTable(file);
            var parser = new BcsReportParser(file.UserId, table, accounts, derivatives);

            var reports = await reportRepo.GetSampleAsync(x =>
                x.Id == file.Name
                && x.BrokerId == brokerId
                && x.DateStart <= parser.DateEnd);

            var alreadyReportDates = reports.SelectMany(x =>
            {
                var date = x.DateStart;
                var daysCount = x.DateEnd.DayNumber - x.DateStart.DayNumber;
                var dates = new List<DateOnly>(daysCount) { date };
                while (daysCount > 0)
                {
                    date = date.AddDays(1);
                    dates.Add(date);
                    daysCount--;
                }
                return dates;
            }).Distinct();

            var deals = parser.Deals.Where(x => !alreadyReportDates.Contains(x.Date)).ToArray();
            var events = parser.Events.Where(x => !alreadyReportDates.Contains(x.Date)).ToArray();

            if (deals.Any())
                _ = await dealRepo.CreateRangeAsync(deals, new DealComparer(), parser.AccountName);
            else if (events.Any())
                _ = await eventRepo.CreateRangeAsync(events, new EventComparer(), parser.AccountName);
            else
            {
                logger.LogInfo<BcsGrabber>(nameof(GetDataAsync), file.Name, "New deals and events not found");
                return;
            }

            var report = new Report
            {
                Id = file.Name,
                BrokerId = brokerId,
                DateStart = parser.DateStart,
                DateEnd = parser.DateEnd,
                ContentType = file.ContentType,
                Payload = file.Payload
            };
            await reportRepo.CreateAsync(report, report.Id);
        }
        catch (Exception exception)
        {
            logger.LogError<BcsGrabber>(nameof(GetDataAsync), exception);
            // automatically creating account
            if (exception.Message.IndexOf("Agreement '", StringComparison.Ordinal) > -1)
            {
                var values = exception.Message.Split('\'');
                var agreement = values[1];
                await accountRepo.CreateAsync(new Account
                {
                    Name = agreement,
                    BrokerId = brokerId,
                    UserId = file.UserId
                }, $"{agreement} created automatically");
            }
        }
    }
    private static DataTable GetExcelTable(ReportFileDto file)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var stream = new MemoryStream(file.Payload);
        using var reader = ExcelReaderFactory.CreateBinaryReader(stream);
        var dataSet = reader.AsDataSet();
        var table = dataSet.Tables[0];
        stream.Close();
        return table;
    }
}

internal sealed class BcsReportParser
{
    internal string AccountName { get; }
    internal DateOnly DateStart { get; }
    internal DateOnly DateEnd { get; }
    internal List<Deal> Deals { get; }
    internal List<Event> Events { get; }

    private readonly int accountId;
    private readonly string userId;
    private const byte brokerId = (byte)Brokers.Bcs;

    private int rowId;
    private readonly DataTable table;

    private readonly IFormatProvider culture;
    private readonly Dictionary<string, int> reportPoints;

    private readonly Dictionary<string, string[]> derivatives;

    private readonly Dictionary<string, Action<string, Currencies?>> reportActionPatterns;

    internal BcsReportParser(string userId, DataTable table, Dictionary<string, int> accounts, Dictionary<string, string[]> derivatives)
    {
        this.userId = userId;
        this.derivatives = derivatives;
        this.table = table;
        culture = new CultureInfo("ru-RU");
        Deals = new List<Deal>(table.Rows.Count);
        Events = new List<Event>(table.Rows.Count);

        reportPoints = new Dictionary<string, int>(BcsReportStructure.Points.Length);

        while (!TryGetCellValue(1, "Дата составления отчета:", 1, out var pointValue))
            if (pointValue is not null && BcsReportStructure.Points.Select(x => pointValue.IndexOf(x, StringComparison.OrdinalIgnoreCase)).Any(x => x > -1))
                reportPoints.Add(pointValue, rowId);

        if (!reportPoints.Any())
            throw new ApplicationException("Report structure not recognized");

        rowId = 0;
        string? _period;
        while (!TryGetCellValue(1, "Период:", 5, out _period))
            continue;

        if (_period is null)
            throw new ApplicationException($"Agreement period '{_period}' not recognized");

        var dates = _period.Split(' ');
        DateStart = DateOnly.Parse(dates[1], culture);
        DateEnd = DateOnly.Parse(dates[3], culture);

        string? _agreement;
        while (!TryGetCellValue(1, "Генеральное соглашение:", 5, out _agreement))
            continue;

        if (_agreement is null || !accounts.ContainsKey(_agreement))
            throw new ApplicationException($"Agreement '{_agreement}' not recognized");

        accountId = accounts[_agreement];
        AccountName = _agreement;

        reportActionPatterns = new(BcsReportStructure.Actions.Length, StringComparer.OrdinalIgnoreCase)
        {
            { BcsReportStructure.Actions[0], ParseDividend },
            { BcsReportStructure.Actions[1], ParseComission },
            { BcsReportStructure.Actions[2], ParseComission },
            { BcsReportStructure.Actions[3], ParseComission },
            { BcsReportStructure.Actions[4], ParseComission },
            { BcsReportStructure.Actions[5], ParseComission },
            { BcsReportStructure.Actions[6], CheckComission },
            { BcsReportStructure.Actions[7], ParseAccountBalance },
            { BcsReportStructure.Actions[8], ParseAccountBalance },
            { BcsReportStructure.Actions[9], ParseStockTransactions },
            { BcsReportStructure.Actions[10], ParseExchangeRate },
            { BcsReportStructure.Actions[11], ParseComission },
            { BcsReportStructure.Actions[12], ParseComission },
            { BcsReportStructure.Actions[13], ParseComission },
            { BcsReportStructure.Actions[14], ParseComission },
            { BcsReportStructure.Actions[15], ParseComission },
            { BcsReportStructure.Actions[16], ParseAdditionalStockRelease }
        };

        Run();
    }

    private void Run()
    {
        string? cellValue;

        var firstBlock = reportPoints.Keys.FirstOrDefault(x => x.IndexOf(BcsReportStructure.Points[0], StringComparison.OrdinalIgnoreCase) > -1);
        if (firstBlock is not null)
        {
            rowId = reportPoints[firstBlock];

            var border = reportPoints.Skip(1).First().Key;

            var rowNo = rowId;
            while (!TryGetCellValue(++rowNo, 1, border, 1, out cellValue))
                if (cellValue is not null)
                    switch (cellValue)
                    {
                        case "USD": GetAction("USD", Currencies.Usd); break;
                        case "Рубль": GetAction("Рубль", Currencies.Rub); break;
                    }

            void GetAction(string value, Currencies? currency)
            {
                while (!TryGetCellValue(1, new[] { $"Итого по валюте {value}:", border }, 2, out cellValue))
                    if (cellValue is not null && reportActionPatterns.ContainsKey(cellValue))
                        reportActionPatterns[cellValue](cellValue, currency);
            }
        }

        var secondBlock = reportPoints.Keys.FirstOrDefault(x => x.IndexOf(BcsReportStructure.Points[2], StringComparison.OrdinalIgnoreCase) > -1);
        if (secondBlock is not null)
        {
            rowId = reportPoints[secondBlock] + 3;

            while (!TryGetCellValue(1, "Итого по валюте Рубль:", 1, out cellValue))
                if (cellValue is not null)
                    reportActionPatterns[BcsReportStructure.Points[2]](cellValue, null);
        }

        var thirdBlock = reportPoints.Keys.FirstOrDefault(x => x.IndexOf(BcsReportStructure.Points[3], StringComparison.OrdinalIgnoreCase) > -1);
        if (thirdBlock is not null)
        {
            rowId = reportPoints[thirdBlock];
            var borders = reportPoints.Keys
                .Where(x => BcsReportStructure.Points[4].IndexOf(x, StringComparison.OrdinalIgnoreCase) > -1 || BcsReportStructure.Points[5].IndexOf(x, StringComparison.OrdinalIgnoreCase) > -1)
                .ToArray();

            while (!TryGetCellValue(1, borders, 6, out cellValue))
                if (cellValue is not null && reportActionPatterns.ContainsKey(cellValue))
                    reportActionPatterns[cellValue](cellValue, null);
        }

        while (!TryGetCellValue(1, "Дата составления отчета:", 12, out cellValue))
            if (cellValue is not null && reportActionPatterns.ContainsKey(cellValue))
                reportActionPatterns[cellValue](GetCellValue(1)!, null);
    }

    private void ParseDividend(string value, Currencies? currency)
    {
        if (!currency.HasValue)
            throw new ArgumentNullException(nameof(currency));

        var info = GetCellValue(14);

        if (info is null)
            throw new Exception(nameof(ParseDividend) + ". Info not found");

        var infoArray = info.Split(',').Select(x => x.Trim());

        var derivativeId = derivatives.Keys.Intersect(infoArray).FirstOrDefault();

        if (derivativeId is null)
            throw new Exception(nameof(ParseDividend) + $". Derivative from '{info}' not found");

        var derivativeCode = derivatives[derivativeId].First();

        decimal tax = 0;
        var taxPosition = info.IndexOf("налог", StringComparison.OrdinalIgnoreCase);
        if (taxPosition > -1)
        {
            var taxRow = info[taxPosition..].Split(' ')[1];
            taxRow = taxRow.IndexOf('$') > -1 ? taxRow[1..] : taxRow;
            tax = decimal.Parse(taxRow, NumberStyles.Number, culture);
        }

        var dateTime = DateOnly.Parse(GetCellValue(1)!, culture);
        var exchangeId = GetExchangeId();

        Events.Add(new Event
        {
            Date = dateTime,
            Cost = decimal.Parse(GetCellValue(6)!),
            Info = info,
            EventTypeId = (byte)EventTypes.Дивиденд,
            DerivativeId = derivativeId,
            DerivativeCode = derivativeCode,
            ExchangeId = exchangeId,
            AccountId = accountId,
            UserId = userId,
            BrokerId = brokerId,
            CurrencyId = (byte)currency
        });
        Events.Add(new Event
        {
            Date = dateTime,
            Cost = tax,
            Info = info,
            EventTypeId = (byte)EventTypes.Налог_с_дивиденда,
            DerivativeId = derivativeId,
            DerivativeCode = derivativeCode,
            ExchangeId = exchangeId,
            AccountId = accountId,
            UserId = userId,
            BrokerId = brokerId,
            CurrencyId = (byte)currency
        });
    }
    private void ParseComission(string value, Currencies? currency)
    {
        if (!currency.HasValue)
            throw new ArgumentNullException(nameof(currency));

        Events.Add(new Event
        {
            Date = DateOnly.Parse(GetCellValue(1)!, culture),
            Cost = decimal.Parse(GetCellValue(7)!),
            Info = GetCellValue(14),
            EventTypeId = (byte)BcsReportStructure.EventTypes[value],
            ExchangeId = GetExchangeId(),
            AccountId = accountId,
            UserId = userId,
            BrokerId = brokerId,
            CurrencyId = (byte)currency
        });
    }
    private void CheckComission(string value, Currencies? currency = null)
    {
        if (!reportActionPatterns.ContainsKey(value))
            throw new ApplicationException(nameof(CheckComission) + $". {nameof(EventType)} '{value}' not found");
    }
    private void ParseAccountBalance(string value, Currencies? currency)
    {
        if (!currency.HasValue)
            throw new ArgumentNullException(nameof(currency));

        var costRowIndex = BcsReportStructure.EventTypes[value] switch
        {
            EventTypes.Пополнение_счета => 6,
            EventTypes.Вывод_с_счета => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(ParseAccountBalance) + $". {nameof(EventType)} not recognized")
        };

        Events.Add(new Event
        {
            Date = DateOnly.Parse(GetCellValue(1)!, culture),
            Cost = decimal.Parse(GetCellValue(costRowIndex)!),
            Info = value,
            EventTypeId = (byte)BcsReportStructure.EventTypes[value],
            ExchangeId = GetExchangeId(),
            AccountId = accountId,
            UserId = userId,
            BrokerId = brokerId,
            CurrencyId = (byte)currency
        });
    }
    private void ParseExchangeRate(string value, Currencies? currency = null)
    {
        var code = GetCellValue(1);

        if (code is null)
            throw new ApplicationException(nameof(ParseExchangeRate) + ". Code not found");

        var (derivativeId, derivativeCodes) = derivatives.FirstOrDefault(x => x.Value.Contains(code, StringComparer.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(derivativeId))
            throw new ApplicationException(nameof(ParseExchangeRate) + $". Derivative '{code}' not found");

        var derivativeCode = derivativeCodes.First(x => x.Equals(code, StringComparison.OrdinalIgnoreCase));

        var buyCurrencyId = (byte)BcsReportStructure.ExchangeCurrencyTypes[derivativeCode].buy;
        var sellCurrencyId = (byte)BcsReportStructure.ExchangeCurrencyTypes[derivativeCode].sell;

        while (!TryGetCellValue(1, $"Итого по {derivativeCode}:", 5, out var cellBuyValue))
        {
            var date = DateOnly.Parse(GetCellValue(1)!, culture);
            var exchange = GetCellValue(14);
            var exchangeId = exchange is null
                ? throw new ApplicationException(nameof(ParseExchangeRate) + ". Exchange not found")
                : (byte)BcsReportStructure.ExchangeTypes[exchange];

            if (!string.IsNullOrWhiteSpace(cellBuyValue))
                Deals.Add(new Deal
                {
                    Date = date,
                    Cost = decimal.Parse(GetCellValue(4)!),
                    Value = decimal.Parse(cellBuyValue),
                    Info = derivativeCode,
                    DerivativeId = derivativeId,
                    DerivativeCode = derivativeCode,
                    ExchangeId = exchangeId,
                    AccountId = accountId,
                    UserId = userId,
                    BrokerId = brokerId,
                    OperationId = (byte)OperationTypes.Расход,
                    CurrencyId = buyCurrencyId
                });
            else
                Deals.Add(new Deal
                {
                    Date = date,
                    Cost = decimal.Parse(GetCellValue(7)!),
                    Value = decimal.Parse(GetCellValue(8)!),
                    Info = derivativeCode,
                    DerivativeId = derivativeId,
                    DerivativeCode = derivativeCode,
                    ExchangeId = exchangeId,
                    AccountId = accountId,
                    UserId = userId,
                    BrokerId = brokerId,
                    OperationId = (byte)OperationTypes.Приход,
                    CurrencyId = sellCurrencyId
                });
        }
    }
    private void ParseStockTransactions(string value, Currencies? currency = null)
    {
        var isin = GetCellValue(7);

        if (isin is null)
            throw new ApplicationException(nameof(ParseStockTransactions) + ". Isin not found");

        var infoArray = isin.Split(',').Select(x => x.Trim());

        var derivativeId = derivatives.Keys.Intersect(infoArray).FirstOrDefault();

        if (derivativeId is null)
            throw new ApplicationException(nameof(ParseStockTransactions) + $". Derivative '{isin}' not found");

        var name = GetCellValue(1);
        while (!TryGetCellValue(1, $"Итого по {name}:", 4, out var cellBuyValue))
        {
            var date = DateOnly.Parse(GetCellValue(1)!, culture);
            currency = GetCellValue(10) switch
            {
                "USD" => Currencies.Usd,
                "Рубль" => Currencies.Rub,
                _ => throw new ArgumentOutOfRangeException(nameof(ParseStockTransactions) + '.' + $" Currency {currency} not found")
            };

            var exchange = GetCellValue(17);
            var exchangeId = exchange is null
                ? throw new ApplicationException(nameof(ParseStockTransactions) + ". Exchange not found")
                : (byte)BcsReportStructure.ExchangeTypes[exchange];

            var derivativeCode = derivatives[derivativeId].First();

            if (!string.IsNullOrWhiteSpace(cellBuyValue))
                Deals.Add(new Deal
                {
                    Date = date,
                    Cost = decimal.Parse(GetCellValue(5)!),
                    Value = decimal.Parse(cellBuyValue),
                    Info = name,
                    DerivativeId = derivativeId,
                    DerivativeCode = derivativeCode,
                    ExchangeId = exchangeId,
                    AccountId = accountId,
                    UserId = userId,
                    BrokerId = brokerId,
                    OperationId = (byte)OperationTypes.Расход,
                    CurrencyId = (byte)currency
                });
            else
                Deals.Add(new Deal
                {
                    Date = date,
                    Cost = decimal.Parse(GetCellValue(8)!),
                    Value = decimal.Parse(GetCellValue(7)!),
                    Info = name,
                    DerivativeId = derivativeId,
                    DerivativeCode = derivativeCode,
                    ExchangeId = exchangeId,
                    AccountId = accountId,
                    UserId = userId,
                    BrokerId = brokerId,
                    OperationId = (byte)OperationTypes.Приход,
                    CurrencyId = (byte)currency
                });
        }
    }
    private void ParseAdditionalStockRelease(string value, Currencies? currency = null)
    {
        var ticker = value.Trim();

        var (derivativeId, derivativeCodes) = derivatives.FirstOrDefault(x => x.Value.Contains(ticker, StringComparer.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(derivativeId))
            throw new ApplicationException(nameof(ParseAdditionalStockRelease) + $". Ticker '{ticker}' not found");

        var derivativeCode = derivativeCodes.First(x => x.Equals(ticker, StringComparison.OrdinalIgnoreCase));

        Deals.Add(new Deal
        {
            Date = DateOnly.Parse(GetCellValue(4)!, culture),
            Cost = 0,
            Value = decimal.Parse(GetCellValue(7)!),
            Info = GetCellValue(12),
            DerivativeId = derivativeId,
            DerivativeCode = derivativeCode,
            ExchangeId = (byte)Exchanges.Spbex,
            AccountId = accountId,
            UserId = userId,
            BrokerId = brokerId,
            OperationId = (byte)OperationTypes.Приход,
            CurrencyId = (byte)Currencies.Usd
        });
    }

    private byte? GetExchangeId()
    {
        var exchange = GetCellValue(12);
        if (string.IsNullOrWhiteSpace(exchange))
            exchange = GetCellValue(11);
        if (string.IsNullOrWhiteSpace(exchange))
            exchange = GetCellValue(10);

        return string.IsNullOrWhiteSpace(exchange) ? null : (byte)BcsReportStructure.ExchangeTypes[exchange];

    }
    private string? GetCellValue(int columnNo) => table.Rows[rowId].ItemArray[columnNo]?.ToString();
    private bool TryGetCellValue(int rowNo, int stopColumnNo, string stopPattern, int targetColumnNo, out string? currentValue)
    {
        var foundingCell = table.Rows[rowNo].ItemArray[stopColumnNo]?.ToString();
        currentValue = table.Rows[rowNo].ItemArray[targetColumnNo]?.ToString();

        return foundingCell is not null && foundingCell.IndexOf(stopPattern, StringComparison.OrdinalIgnoreCase) > -1;
    }
    private bool TryGetCellValue(int stopColumnNo, string stopPattern, int targetColumnNo, out string? currentValue)
    {
        rowId++;

        var foundingCell = table.Rows[rowId].ItemArray[stopColumnNo]?.ToString();
        currentValue = table.Rows[rowId].ItemArray[targetColumnNo]?.ToString();

        return foundingCell is not null && foundingCell.IndexOf(stopPattern, StringComparison.OrdinalIgnoreCase) > -1;
    }
    private bool TryGetCellValue(int stopColumnNo, IEnumerable<string> stopPatterns, int targetColumnNo, out string? currentValue)
    {
        rowId++;

        var foundingCell = table.Rows[rowId].ItemArray[stopColumnNo]?.ToString();
        currentValue = table.Rows[rowId].ItemArray[targetColumnNo]?.ToString();

        return foundingCell is not null
               && stopPatterns
                   .Select(x => foundingCell
                       .IndexOf(x, StringComparison.OrdinalIgnoreCase))
                   .Any(x => x > -1);
    }
}
internal static class BcsReportStructure
{
    internal static readonly string[] Points = {
        "1.1.1. Движение денежных средств по совершенным сделкам (иным операциям) с ценными бумагами",
        "1.2. Займы:",
        "сборы/штрафы (итоговые суммы):",
        "2.1. Сделки:",
        "2.3. Незавершенные сделки",
        "3. Активы:"
    };
    internal static readonly string[] Actions = {
        "Дивиденды",
        "Урегулирование сделок",
        "Вознаграждение компании",
        "Вознаграждение за обслуживание счета депо",
        "Хранение ЦБ",
        "НДФЛ",
        "сборы/штрафы (итоговые суммы):",
        "Приход ДС",
        "Вывод ДС",
        "ISIN:",
        "Сопряж. валюта:",
        "Вознаграждение компании (СВОП)",
        "Комиссия за займы \"овернайт ЦБ\"",
        "Вознаграждение компании (репо)",
        "Комиссия Биржевой гуру",
        "Оплата за вывод денежных средств",
        "Доп. выпуск акций "
    };

    private static readonly string[] Exchanges =
    {
        "МосБирж(Валютный рынок)",
        "ММВБ",
        "СПБ",
        "МосБирж(FORTS)",
        "Внебирж."
    };
    private static readonly string[] ExchangeCurrencies =
    {
        "USDRUB_TOD",
        "USDRUB_TOM"
    };

    internal static readonly Dictionary<string, EventTypes> EventTypes = new(Actions.Length, StringComparer.OrdinalIgnoreCase)
    {
        { Actions[1], Enums.EventTypes.Комиссия_брокера },
        { Actions[2], Enums.EventTypes.Комиссия_брокера },
        { Actions[3], Enums.EventTypes.Комиссия_депозитария },
        { Actions[4], Enums.EventTypes.Комиссия_депозитария },
        { Actions[5], Enums.EventTypes.НДФЛ },
        { Actions[7], Enums.EventTypes.Пополнение_счета },
        { Actions[8], Enums.EventTypes.Вывод_с_счета },
        { Actions[11], Enums.EventTypes.Комиссия_брокера },
        { Actions[12], Enums.EventTypes.Комиссия_брокера },
        { Actions[13], Enums.EventTypes.Комиссия_брокера },
        { Actions[14], Enums.EventTypes.Комиссия_брокера },
        { Actions[15], Enums.EventTypes.Комиссия_брокера }
    };
    internal static readonly Dictionary<string, Exchanges> ExchangeTypes = new(Exchanges.Length, StringComparer.OrdinalIgnoreCase)
    {
        { Exchanges[0], Shared.Enums.Exchanges.Moex },
        { Exchanges[1], Shared.Enums.Exchanges.Moex },
        { Exchanges[2], Shared.Enums.Exchanges.Spbex },
        { Exchanges[3], Shared.Enums.Exchanges.Moex },
        { Exchanges[4], Shared.Enums.Exchanges.Moex }
    };
    internal static readonly Dictionary<string, (Currencies buy, Currencies sell)> ExchangeCurrencyTypes = new(ExchangeCurrencies.Length, StringComparer.OrdinalIgnoreCase)
    {
        { ExchangeCurrencies[0], (Currencies.Usd, Currencies.Rub) },
        { ExchangeCurrencies[1], (Currencies.Usd, Currencies.Rub) }
    };
}