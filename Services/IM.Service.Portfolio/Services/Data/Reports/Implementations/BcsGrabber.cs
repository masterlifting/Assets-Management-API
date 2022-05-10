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

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helpers.LogHelper;
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
        var accountNames = await accountRepo.GetSampleAsync(x => x.BrokerId == brokerId && x.UserId == file.UserId, x => x.Name);
        var derivatives = await derivativeRepo.GetSampleAsync(x => ValueTuple.Create(x.Id, x.Code));

        try
        {
            var table = GetExcelTable(file);
            var parser = new BcsReportParser(file.UserId, table, accountNames, derivatives.ToDictionary(x => x.Item1, x => x.Item2));

            var reports = await reportRepo.GetSampleAsync(x =>
                x.AccountUserId == file.UserId
                && x.AccountBrokerId == brokerId
                && x.AccountName == parser.AccountName
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

            var deals = parser.Deals.Where(x => !alreadyReportDates.Contains(DateOnly.FromDateTime(x.DateTime))).ToArray();
            var events = parser.Events.Where(x => !alreadyReportDates.Contains(DateOnly.FromDateTime(x.DateTime))).ToArray();

            if (deals.Any())
            {
                foreach (var deal in deals)
                    deal.DateTime = deal.DateTime.ToUniversalTime();

                _ = await dealRepo.CreateRangeAsync(deals, new DealComparer(), string.Join("; ", deals.Select(x => x.AccountName).Distinct()));
            }
            else if (events.Any())
            {
                foreach (var _event in events)
                    _event.DateTime = _event.DateTime.ToUniversalTime();

                _ = await eventRepo.CreateRangeAsync(events, new EventComparer(), string.Join("; ", events.Select(x => x.AccountName).Distinct()));
            }
            else
            {
                logger.LogInfo(nameof(GetDataAsync), file.Name, "report is already");
                return;
            }

            var report = new Report
            {
                AccountName = parser.AccountName,
                AccountUserId = file.UserId,
                AccountBrokerId = brokerId,
                DateStart = parser.DateStart,
                DateEnd = parser.DateEnd,
                Name = file.Name,
                ContentType = file.ContentType,
                Payload = file.Payload
            };
            await reportRepo.CreateAsync(report, report.Name);
        }
        catch (Exception exception)
        {
            logger.LogError(nameof(GetDataAsync), exception);

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
                }, agreement);
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


    private readonly IFormatProvider culture;

    private readonly Dictionary<string, int> reportPointPatterns;
    private readonly Dictionary<string, Action<Currencies, string>> reportActionPatterns;

    private readonly Dictionary<string, EventTypes> eventTypes;
    private readonly Dictionary<string, Exchanges> exchangeTypes;
    private readonly Dictionary<string, (Currencies buy, Currencies sell)> exchangeCurrencyTypes;

    private const byte brokerId = (byte)Brokers.Bcs;
    private readonly string userId;
    private int rowId;
    private readonly Dictionary<string, string> derivatives;
    private readonly DataTable table;

    internal BcsReportParser(string userId, DataTable table, IEnumerable<string> accounts, Dictionary<string, string> derivatives)
    {
        this.derivatives = derivatives;
        this.userId = userId;
        this.table = table;
        culture = new CultureInfo("ru-RU");
        Deals = new List<Deal>(table.Rows.Count);
        Events = new List<Event>(table.Rows.Count);

        reportPointPatterns = new Dictionary<string, int>(BcsReportStructure.Points.Length);

        while (!TryCellValue(1, "Дата составления отчета:", 1, out var pointValue))
            if (pointValue is not null && BcsReportStructure.Points.Select(x => pointValue.IndexOf(x, StringComparison.OrdinalIgnoreCase)).Any(x => x > -1))
                reportPointPatterns.Add(pointValue, rowId);

        if (!reportPointPatterns.Any())
            throw new ApplicationException("Report structure not recognized");

        rowId = 0;
        string? _period;
        while (!TryCellValue(1, "Период:", 5, out _period))
            continue;

        if (_period is null)
            throw new ApplicationException($"Agreement period '{_period}' not recognized");

        var dates = _period.Split(' ');
        DateStart = DateOnly.Parse(dates[1], culture);
        DateEnd = DateOnly.Parse(dates[3], culture);

        string? _agreement;
        while (!TryCellValue(1, "Генеральное соглашение:", 5, out _agreement))
            continue;

        var account = accounts.FirstOrDefault(x => x.Equals(_agreement, StringComparison.OrdinalIgnoreCase));
        AccountName = _agreement is not null && account is not null
                    ? account
                    : throw new ApplicationException($"Agreement '{_agreement}' not recognized");

        reportActionPatterns = new(StringComparer.OrdinalIgnoreCase)
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
        eventTypes = new()
        {
            { BcsReportStructure.Actions[1], EventTypes.Комиссия_брокера },
            { BcsReportStructure.Actions[2], EventTypes.Комиссия_брокера },
            { BcsReportStructure.Actions[3], EventTypes.Комиссия_депозитария },
            { BcsReportStructure.Actions[4], EventTypes.Комиссия_депозитария },
            { BcsReportStructure.Actions[5], EventTypes.НДФЛ },
            { BcsReportStructure.Actions[7], EventTypes.Пополнение_счета },
            { BcsReportStructure.Actions[8], EventTypes.Вывод_с_счета },
            { BcsReportStructure.Actions[11], EventTypes.Комиссия_брокера },
            { BcsReportStructure.Actions[12], EventTypes.Комиссия_брокера },
            { BcsReportStructure.Actions[13], EventTypes.Комиссия_брокера },
            { BcsReportStructure.Actions[14], EventTypes.Комиссия_брокера },
            { BcsReportStructure.Actions[15], EventTypes.Комиссия_брокера }
        };
        exchangeTypes = new()
        {
            { BcsReportStructure.Exchanges[0], Exchanges.Moex },
            { BcsReportStructure.Exchanges[1], Exchanges.Moex },
            { BcsReportStructure.Exchanges[2], Exchanges.Spbex }
        };
        exchangeCurrencyTypes = new()
        {
            { BcsReportStructure.ExchangeCurrencies[0], (Currencies.Usd, Currencies.Rub) },
            { BcsReportStructure.ExchangeCurrencies[1], (Currencies.Usd, Currencies.Rub) }
        };

        Run();
    }

    private void Run()
    {
        string? cellValue;

        var firstBlock = reportPointPatterns.Keys.FirstOrDefault(x => x.IndexOf(BcsReportStructure.Points[0], StringComparison.OrdinalIgnoreCase) > -1);
        if (firstBlock is not null)
        {
            rowId = reportPointPatterns[firstBlock];

            var border = reportPointPatterns.Skip(1).First().Key;

            var rowNo = rowId;
            while (!TryCellValue(++rowNo, 1, border, 1, out cellValue))
                if (cellValue is not null)
                    switch (cellValue)
                    {
                        case "USD": GetAction(Currencies.Usd, "USD"); break;
                        case "Рубль": GetAction(Currencies.Rub, "Рубль"); break;
                    }

            void GetAction(Currencies currency, string value)
            {
                while (!TryCellValue(1, new[] { $"Итого по валюте {value}:", border }, 2, out cellValue))
                    if (cellValue is not null && reportActionPatterns.ContainsKey(cellValue))
                        reportActionPatterns[cellValue](currency, cellValue);
            }
        }

        var secondBlock = reportPointPatterns.Keys.FirstOrDefault(x => x.IndexOf(BcsReportStructure.Points[2], StringComparison.OrdinalIgnoreCase) > -1);
        if (secondBlock is not null)
        {
            rowId = reportPointPatterns[secondBlock] + 3;

            while (!TryCellValue(1, "Итого по валюте Рубль:", 1, out cellValue))
                if (cellValue is not null)
                    reportActionPatterns[BcsReportStructure.Points[2]](Currencies.Default, cellValue);
        }

        var thirdBlock = reportPointPatterns.Keys.FirstOrDefault(x => x.IndexOf(BcsReportStructure.Points[3], StringComparison.OrdinalIgnoreCase) > -1);
        if (thirdBlock is not null)
        {
            rowId = reportPointPatterns[thirdBlock];
            var borders = reportPointPatterns.Keys
                .Where(x => BcsReportStructure.Points[4].IndexOf(x, StringComparison.OrdinalIgnoreCase) > -1 || BcsReportStructure.Points[5].IndexOf(x, StringComparison.OrdinalIgnoreCase) > -1)
                .ToArray();

            while (!TryCellValue(1, borders, 6, out cellValue))
                if (cellValue is not null && reportActionPatterns.ContainsKey(cellValue))
                    reportActionPatterns[cellValue](Currencies.Default, cellValue);
        }

        while (!TryCellValue(1, "Дата составления отчета:", 12, out cellValue))
            if (cellValue is not null && reportActionPatterns.ContainsKey(cellValue))
                reportActionPatterns[cellValue](Currencies.Default, GetCellValue(1)!);
    }

    private void ParseDividend(Currencies currency, string value)
    {
        var info = GetCellValue(14);

        if (info is null)
            throw new Exception(nameof(ParseDividend) + ". Info not found");

        var infoArray = info.Split(',').Select(x => x.Trim());

        var derivativeId = derivatives.Keys.Intersect(infoArray).FirstOrDefault();

        if (derivativeId is null)
            throw new Exception(nameof(ParseDividend) + $". Derivative from '{info}' not found");

        var derivativeCode = derivatives[derivativeId];

        decimal tax = 0;
        var taxPosition = info.IndexOf("налог", StringComparison.OrdinalIgnoreCase);
        if (taxPosition > -1)
        {
            var taxRow = info[taxPosition..].Split(' ')[1];
            taxRow = taxRow.IndexOf('$') > -1 ? taxRow[1..] : taxRow;
            tax = decimal.Parse(taxRow, NumberStyles.Number, culture);
        }

        var dateTime = DateTime.Parse(GetCellValue(1)!, culture);
        var exchange = GetCellValue(12);
        byte? exchangeId = exchange is null ? null : (byte)exchangeTypes[exchange];

        Events.Add(new Event
        {
            DateTime = dateTime,
            Cost = decimal.Parse(GetCellValue(6)!),
            Info = info,
            EventTypeId = (byte)EventTypes.Дивиденд,
            DerivativeId = derivativeId,
            DerivativeCode = derivativeCode,
            ExchangeId = exchangeId,
            AccountBrokerId = brokerId,
            AccountUserId = userId,
            AccountName = AccountName,
            CurrencyId = (byte)currency
        });
        Events.Add(new Event
        {
            DateTime = dateTime,
            Cost = tax,
            Info = info,
            EventTypeId = (byte)EventTypes.Налог_с_дивиденда,
            DerivativeId = derivativeId,
            DerivativeCode = derivativeCode,
            ExchangeId = exchangeId,
            AccountBrokerId = brokerId,
            AccountUserId = userId,
            AccountName = AccountName,
            CurrencyId = (byte)currency
        });
    }
    private void ParseComission(Currencies currency, string value)
    {
        var info = GetCellValue(14);
        var exchange = GetCellValue(12);
        byte? exchangeId = exchange is null ? null : (byte)exchangeTypes[exchange];

        Events.Add(new Event
        {
            DateTime = DateTime.Parse(GetCellValue(1)!, culture),
            Cost = decimal.Parse(GetCellValue(7)!),
            Info = info,
            EventTypeId = (byte)eventTypes[value],
            ExchangeId = exchangeId,
            AccountBrokerId = brokerId,
            AccountUserId = userId,
            AccountName = AccountName,
            CurrencyId = (byte)currency
        });
    }
    private void CheckComission(Currencies currency, string value)
    {
        if (!reportActionPatterns.ContainsKey(value))
            throw new ApplicationException(nameof(CheckComission) + $". {nameof(EventType)} '{value}' not found");
    }
    private void ParseAccountBalance(Currencies currency, string value)
    {
        var costRowIndex = eventTypes[value] switch
        {
            EventTypes.Пополнение_счета => 6,
            EventTypes.Вывод_с_счета => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(ParseAccountBalance) + $". {nameof(EventType)} not recognized")
        };

        var exchange = GetCellValue(12);
        byte? exchangeId = exchange is null ? null : (byte)exchangeTypes[exchange];

        Events.Add(new Event
        {
            DateTime = DateTime.Parse(GetCellValue(1)!, culture),
            Cost = decimal.Parse(GetCellValue(costRowIndex)!),
            Info = value,
            EventTypeId = (byte)eventTypes[value],
            ExchangeId = exchangeId,
            AccountBrokerId = brokerId,
            AccountUserId = userId,
            AccountName = AccountName,
            CurrencyId = (byte)currency
        });
    }
    private void ParseExchangeRate(Currencies currency, string value)
    {
        var code = GetCellValue(1);

        if (code is null)
            throw new ApplicationException(nameof(ParseExchangeRate) + ". Code not found");

        if (!derivatives.ContainsValue(code))
            throw new ApplicationException(nameof(ParseExchangeRate) + $". Derivative '{code}' not found");

        var (derivativeId, derivativeCode) = derivatives.First(x => x.Value.Equals(code));

        var buyCurrencyId = (byte)exchangeCurrencyTypes[derivativeCode].buy;
        var sellCurrencyId = (byte)exchangeCurrencyTypes[derivativeCode].sell;

        while (!TryCellValue(1, $"Итого по {derivativeCode}:", 5, out var cellBuyValue))
        {
            var date = DateTime.Parse(GetCellValue(1)!, culture);
            var exchange = GetCellValue(14);
            var exchangeId = exchange is null
                ? throw new ApplicationException(nameof(ParseExchangeRate) + ". Exchange not found")
                : (byte)exchangeTypes[exchange];

            if (!string.IsNullOrWhiteSpace(cellBuyValue))
                Deals.Add(new Deal
                {
                    DateTime = date,
                    Cost = decimal.Parse(GetCellValue(4)!),
                    Value = decimal.Parse(cellBuyValue),
                    Info = derivativeCode,
                    DerivativeId = derivativeId,
                    DerivativeCode = derivativeCode,
                    ExchangeId = exchangeId,
                    AccountBrokerId = brokerId,
                    AccountUserId = userId,
                    AccountName = AccountName,
                    OperationId = (byte)OperationTypes.Расход,
                    CurrencyId = buyCurrencyId
                });
            else
                Deals.Add(new Deal
                {
                    DateTime = date,
                    Cost = decimal.Parse(GetCellValue(7)!),
                    Value = decimal.Parse(GetCellValue(8)!),
                    Info = derivativeCode,
                    DerivativeId = derivativeId,
                    DerivativeCode = derivativeCode,
                    ExchangeId = exchangeId,
                    AccountBrokerId = brokerId,
                    AccountUserId = userId,
                    AccountName = AccountName,
                    OperationId = (byte)OperationTypes.Приход,
                    CurrencyId = sellCurrencyId
                });
        }
    }
    private void ParseStockTransactions(Currencies currency, string value)
    {
        var isin = GetCellValue(7);

        if (isin is null)
            throw new ApplicationException(nameof(ParseStockTransactions) + ". Isin not found");

        var infoArray = isin.Split(',').Select(x => x.Trim());

        var derivativeId = derivatives.Keys.Intersect(infoArray).FirstOrDefault();

        if (derivativeId is null)
            throw new ApplicationException(nameof(ParseStockTransactions) + $". Derivative '{isin}' not found");

        var derivativeCode = derivatives[derivativeId];

        var name = GetCellValue(1);
        while (!TryCellValue(1, $"Итого по {name}:", 4, out var cellBuyValue))
        {
            var date = DateTime.Parse(GetCellValue(1)!, culture);
            currency = GetCellValue(10) switch
            {
                "USD" => Currencies.Usd,
                "Рубль" => Currencies.Rub,
                _ => throw new ArgumentOutOfRangeException(nameof(ParseStockTransactions) + '.' + $" Currency {currency} not found")
            };

            var exchange = GetCellValue(17);
            var exchangeId = exchange is null
                ? throw new ApplicationException(nameof(ParseStockTransactions) + ". Exchange not found")
                : (byte)exchangeTypes[exchange];

            if (!string.IsNullOrWhiteSpace(cellBuyValue))
                Deals.Add(new Deal
                {
                    DateTime = date,
                    Cost = decimal.Parse(GetCellValue(5)!),
                    Value = decimal.Parse(cellBuyValue),
                    Info = name,
                    DerivativeId = derivativeId,
                    DerivativeCode = derivativeCode,
                    ExchangeId = exchangeId,
                    AccountBrokerId = brokerId,
                    AccountUserId = userId,
                    AccountName = AccountName,
                    OperationId = (byte)OperationTypes.Расход,
                    CurrencyId = (byte)currency
                });
            else
                Deals.Add(new Deal
                {
                    DateTime = date,
                    Cost = decimal.Parse(GetCellValue(8)!),
                    Value = decimal.Parse(GetCellValue(7)!),
                    Info = name,
                    DerivativeId = derivativeId,
                    DerivativeCode = derivativeCode,
                    ExchangeId = exchangeId,
                    AccountBrokerId = brokerId,
                    AccountUserId = userId,
                    AccountName = AccountName,
                    OperationId = (byte)OperationTypes.Приход,
                    CurrencyId = (byte)currency
                });
        }
    }
    private void ParseAdditionalStockRelease(Currencies currency, string value)
    {
        var ticker = value.Trim();

        if (!derivatives.ContainsValue(ticker))
            throw new ApplicationException(nameof(ParseAdditionalStockRelease) + $". Ticker '{ticker}' not found");

        var (derivativeId, derivativeCode) = derivatives.First(x => x.Value.Equals(ticker));

        Deals.Add(new Deal
        {
            DateTime = DateTime.Parse(GetCellValue(4)!, culture),
            Cost = 0,
            Value = decimal.Parse(GetCellValue(7)!),
            Info = GetCellValue(12),
            DerivativeId = derivativeId,
            DerivativeCode = derivativeCode,
            ExchangeId = 0,
            AccountBrokerId = brokerId,
            AccountUserId = userId,
            AccountName = AccountName,
            OperationId = (byte)OperationTypes.Приход,
            CurrencyId = (byte)currency
        });
    }

    private bool TryCellValue(int rowNo, int stopColumnNo, string stopPattern, int targetColumnNo, out string? currentValue)
    {
        var foundingCell = table.Rows[rowNo].ItemArray[stopColumnNo]?.ToString();
        currentValue = table.Rows[rowNo].ItemArray[targetColumnNo]?.ToString();

        return foundingCell is not null && foundingCell.IndexOf(stopPattern, StringComparison.OrdinalIgnoreCase) > -1;
    }
    private bool TryCellValue(int stopColumnNo, string stopPattern, int targetColumnNo, out string? currentValue)
    {
        rowId++;

        var foundingCell = table.Rows[rowId].ItemArray[stopColumnNo]?.ToString();
        currentValue = table.Rows[rowId].ItemArray[targetColumnNo]?.ToString();

        return foundingCell is not null && foundingCell.IndexOf(stopPattern, StringComparison.OrdinalIgnoreCase) > -1;
    }
    private bool TryCellValue(int stopColumnNo, IEnumerable<string> stopPatterns, int targetColumnNo, out string? currentValue)
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
    private string? GetCellValue(int columnNo) => table.Rows[rowId].ItemArray[columnNo]?.ToString();
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
    internal static readonly string[] Exchanges =
    {
        "МосБирж(Валютный рынок)",
        "ММВБ",
        "СПБ"
    };
    internal static readonly string[] ExchangeCurrencies =
    {
        "USDRUB_TOD",
        "USDRUB_TOM"
    };
}