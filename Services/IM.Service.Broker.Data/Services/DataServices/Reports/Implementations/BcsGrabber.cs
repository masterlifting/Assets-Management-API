using ExcelDataReader;

using IM.Service.Broker.Data.DataAccess.Comparators;
using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Broker.Data.DataAccess.Repository;
using IM.Service.Broker.Data.Models.Dto.Mq;
using IM.Service.Common.Net;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static IM.Service.Broker.Data.Enums;

namespace IM.Service.Broker.Data.Services.DataServices.Reports.Implementations;

public class BcsGrabber : IDataGrabber
{
    private readonly Repository<Report> reportRepository;
    private readonly Repository<Stock> stockRepository;
    private readonly Repository<Transaction> transactionRepository;
    private readonly Repository<Account> accountRepository;
    private readonly ILogger<ReportGrabber> logger;

    public BcsGrabber(
        Repository<Report> reportRepository,
        Repository<Stock> stockRepository,
        Repository<Transaction> transactionRepository,
        Repository<Account> accountRepository,
        ILogger<ReportGrabber> logger)
    {
        this.reportRepository = reportRepository;
        this.transactionRepository = transactionRepository;
        this.accountRepository = accountRepository;
        this.stockRepository = stockRepository;
        this.logger = logger;
    }

    public async Task GrabDataAsync(ReportFileDto file, Brokers broker)
    {
        try
        {
            var accounts = await accountRepository.GetSampleAsync(x => x.BrokerId == (byte)broker && x.UserId == file.UserId);
            var isins = await stockRepository.GetSampleAsync(x => x.Isin);

            if (!isins.Any())
            {
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "LMT",
                    ExchangeId = (byte)Exchanges.Spb,
                    Isin = "US5398301094"
                }, "Lockheed Martin Corporation");
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "DISCK",
                    ExchangeId = (byte)Exchanges.Spb,
                    Isin = "US25470F3029"
                }, "Discovery, Inc.");
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "GAZP",
                    ExchangeId = (byte)Exchanges.Moex,
                    Isin = "RU0007661625"
                }, "Публичное акционерное общество \"Газпром\"");
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "NVTK",
                    ExchangeId = (byte)Exchanges.Moex,
                    Isin = "RU000A0DKVS5"
                }, "публичное акционерное общество \"НОВАТЭК\"");
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "SBER",
                    ExchangeId = (byte)Exchanges.Moex,
                    Isin = "RU0009029540"
                }, "Публичное акционерное общество \"Сбербанк России\"");
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "UNAC",
                    ExchangeId = (byte)Exchanges.Moex,
                    Isin = "RU000A0JPLZ7"
                }, "Публичное акционерное общество \"Объединенная авиастроительная корпорация\"");
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "YY",
                    ExchangeId = (byte)Exchanges.Spb,
                    Isin = "US46591M1099"
                }, "JOYY");
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "POLY",
                    ExchangeId = (byte)Exchanges.Moex,
                    Isin = "JE00B6T5S470"
                }, "Polymetal International plc");
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "MRK",
                    ExchangeId = (byte)Exchanges.Spb,
                    Isin = "US58933Y1055"
                }, "Merck & Co., Inc.");
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "MDT",
                    ExchangeId = (byte)Exchanges.Spb,
                    Isin = "IE00BTN1Y115"
                }, "Medtronic plc");
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "AKRN",
                    ExchangeId = (byte)Exchanges.Moex,
                    Isin = "RU0009028674"
                }, "ПАО \"Акрон\"");
                await stockRepository.CreateAsync(new Stock
                {
                    CompanyId = "PLZL",
                    ExchangeId = (byte)Exchanges.Moex,
                    Isin = "RU000A0JNAA8"
                }, "ПАО \"Полюс\"");

                isins = await stockRepository.GetSampleAsync(x => x.Isin);
            }

            var table = GetTable(file);

            var parser = new BcsReportParser(table, accounts, isins);

            var transactions = parser.GetTransactions();

            var (error, _) = await transactionRepository.CreateUpdateAsync(transactions, new TransactionComparer(), nameof(BcsGrabber));

            if (error is null)
                await reportRepository.CreateAsync(new Report
                {
                    AccountId = parser.AccountId,
                    DateStart = parser.DateStart,
                    DateEnd = parser.DateEnd,
                    Name = file.Name,
                    ContentType = file.ContentType,
                    Payload = file.Payload
                }, file.Name);
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(BcsGrabber), exception.Message);

            if (exception.Message.IndexOf("Agreement '", StringComparison.Ordinal) > -1)
            {
                var values = exception.Message.Split('\'');
                var agreement = values[1];
                await accountRepository.CreateAsync(new Account
                {
                    Name = agreement,
                    BrokerId = (byte)broker,
                    UserId = file.UserId
                }, agreement);
            }
        }
    }

    private static DataTable GetTable(ReportFileDto file)
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

internal class BcsReportParser
{
    internal int AccountId { get; }
    internal DateOnly DateStart { get; }
    internal DateOnly DateEnd { get; }
    
    private readonly string agreement;
    private readonly Dictionary<string, Action<Currencies, string>> parseActions;
    private readonly Dictionary<string, byte> transactionActions = new()
    {
        { "Урегулирование сделок", (byte)TransactionActions.Комиссия_брокера },
        { "Вознаграждение компании", (byte)TransactionActions.Комиссия_брокера },
        { "Вознаграждение за обслуживание счета депо", (byte)TransactionActions.Комиссия_депозитария },
        { "Хранение ЦБ", (byte)TransactionActions.Комиссия_депозитария },
        { "Приход ДС", (byte)TransactionActions.Пополнение_счета },
        { "Вывод ДС", (byte)TransactionActions.Вывод_с_счета }
    };

    private readonly string[] isins;
    private readonly List<Transaction> transactions;

    private readonly IFormatProvider culture = new CultureInfo("ru-RU");
    private readonly DataTable table;
    private int rowId;

    private bool TryCellValue(int columnNo, string pattern, int targetColumnNo, out string? value)
    {
        var foundingCell = table.Rows[rowId].ItemArray[columnNo]?.ToString();
        value = table.Rows[rowId].ItemArray[targetColumnNo]?.ToString();

        return foundingCell is not null && foundingCell.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) > -1;
    }
    private string? GetCellValue(int columnNo) => table.Rows[rowId].ItemArray[columnNo]?.ToString();
    private bool IsCell(int columnNo, string pattern)
    {
        var cell = table.Rows[rowId].ItemArray[columnNo]?.ToString();
        return cell is not null && cell.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) > -1;
    }

    public BcsReportParser(DataTable table, IEnumerable<Account> accounts, IEnumerable<string> isins)
    {
        this.table = table;
        this.isins = isins.ToArray();
        var accountsDictionary = accounts.ToDictionary(x => x.Name, y => y.Id, StringComparer.OrdinalIgnoreCase);

        string? _agreement;
        while (!TryCellValue(1, "Генеральное соглашение:", 5, out _agreement))
            rowId++;

        AccountId = _agreement is not null && accountsDictionary.ContainsKey(_agreement)
                    ? accountsDictionary[_agreement]
                    : throw new Exception($"Agreement '{_agreement}' was not recognized");

        agreement = _agreement;
        transactions = new List<Transaction>(200);
        parseActions = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Дивиденды", ParseDividend },
            { "Урегулирование сделок", ParseComission },
            { "Вознаграждение компании", ParseComission },
            { "Вознаграждение за обслуживание счета депо", ParseComission },
            { "Хранение ЦБ", ParseComission },
            { "Приход ДС", ParseAccountTransactions },
            { "Вывод ДС", ParseAccountTransactions }
        };
    }

    public IEnumerable<Transaction> GetTransactions()
    {
        string? cellValue;

        while (!TryCellValue(1, "Итого по валюте USD:", 2, out cellValue))
        {
            if (cellValue is not null && parseActions.ContainsKey(cellValue))
            {
                parseActions[cellValue](Currencies.Usd, cellValue);
                rowId++;
                continue;
            }

            rowId++;
        }
        while (!TryCellValue(1, "Итого по валюте Рубль:", 2, out cellValue))
        {
            if (cellValue is not null && parseActions.ContainsKey(cellValue))
            {
                parseActions[cellValue](Currencies.Rub, cellValue);
                rowId++;
                continue;
            }

            rowId++;
        }
        while (!TryCellValue(1, "3. Активы:", 2, out cellValue))
        {
            if (cellValue is not null && parseActions.ContainsKey(cellValue))
            {
                parseActions[cellValue](Currencies.Rub, cellValue);
                rowId++;
                continue;
            }

            rowId++;
        }

        return transactions;
    }

    private void ParseDividend(Currencies currency, string value)
    {
        var info = GetCellValue(14);

        if (info is null)
            throw new Exception(nameof(ParseDividend) + '.' + $" Agreement: '{agreement}'. Info about tax was not found");

        var infoArray = info.Split(',').Select(x => x.Trim());

        var isin = isins.Intersect(infoArray).FirstOrDefault();

        if (isin is null || !isins.Contains(isin))
            throw new Exception(nameof(ParseDividend) + '.' + $" Agreement: '{agreement}'. Isin from '{info}' was not found");

        decimal tax = 0;
        var taxPosition = info.IndexOf("налог", StringComparison.OrdinalIgnoreCase);
        if (taxPosition > -1)
        {
            var taxRow = info[taxPosition..].Split(' ')[1][1..];
            tax = decimal.Parse(taxRow, NumberStyles.Number, culture);
        }

        var dateTime = DateTime.Parse(GetCellValue(1)!, culture);

        transactions.Add(new Transaction
        {
            AccountId = AccountId,
            CurrencyId = (byte)currency,
            TransactionActionId = (byte)TransactionActions.Дивиденд,
            DateTime = dateTime,
            Cost = decimal.Parse(GetCellValue(6)!),
            Value = 1,
            Info = info,
            StockIsin = isin
        });

        transactions.Add(new Transaction
        {
            AccountId = AccountId,
            CurrencyId = (byte)currency,
            TransactionActionId = (byte)TransactionActions.Налог_с_дивиденда,
            DateTime = dateTime,
            Cost = tax,
            Value = 1,
            Info = info,
            StockIsin = isin
        });
    }
    private void ParseComission(Currencies currency, string value) =>
        transactions.Add(new Transaction
        {
            AccountId = AccountId,
            CurrencyId = (byte)currency,
            TransactionActionId = transactionActions[value],
            DateTime = DateTime.Parse(GetCellValue(1)!, culture),
            Cost = decimal.Parse(GetCellValue(7)!),
            Value = 1,
            Info = value
        });
    private void ParseAccountTransactions(Currencies currency, string value) =>
        transactions.Add(new Transaction
        {
            AccountId = AccountId,
            CurrencyId = (byte)currency,
            TransactionActionId = transactionActions[value],
            DateTime = DateTime.Parse(GetCellValue(1)!, culture),
            Cost = decimal.Parse(GetCellValue(6)!),
            Value = 1,
            Info = value
        });
    private void ParseExchangeRate(Currencies currency, string value)
    {
        transactions.Add(new Transaction
        {
            AccountId = AccountId,
            CurrencyId = (byte)currency,
            TransactionActionId = transactionActions[value],
            DateTime = DateTime.Parse(GetCellValue(1)!, culture),
            Cost = decimal.Parse(GetCellValue(7)!),
            Value = 1,
            Info = value
        });
    }
}