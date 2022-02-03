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
using static IM.Service.Broker.Data.Services.DataServices.Reports.Implementations.ReportConfiguration;

// ReSharper disable RedundantJumpStatement
// ReSharper disable RedundantAssignment

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
        var accounts = await accountRepository.GetSampleAsync(x => x.BrokerId == (byte)broker && x.UserId == file.UserId);
        var isins = await stockRepository.GetSampleAsync(x => x.Isin);

        if (!isins.Any())
        {
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "GE",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US3696041033"
            }, "General Electric Company");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "AAPL",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US0378331005"
            }, "Apple Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "TSLA",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US88160R1014"
            }, "Tesla, Inc.");
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
            }, "ПАО \"Газпром\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "NVTK",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU000A0DKVS5"
            }, "ПАО \"НОВАТЭК\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "SBER",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU0009029540"
            }, "ПАО \"Сбербанк России\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "UNAC",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU000A0JPLZ7"
            }, "ПАО \"Объединенная авиастроительная корпорация\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "YY",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US46591M1099"
            }, "JOYY");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "YY",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US98426T1060"
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
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "INTC",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US4581401001"
            }, "Intel Corporation");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "V",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US92826C8394"
            }, "Visa Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "PFE",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US7170811035"
            }, "Pfizer Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "IBM",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US4592001014"
            }, "IBM");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "MMM",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US88579Y1010"
            }, "3M Company");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "OGN",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US68622V1061"
            }, "Organon & Co");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "NOC",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US6668071029"
            }, "Northrop Grumman Corporation");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "VTRS",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US92556V1061"
            }, "Viatris Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "GPN",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US37940X1028"
            }, "Global Payments Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "NEM",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US6516391066"
            }, "Newmont Corporation");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "QCOM",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US7475251036"
            }, "QUALCOMM Incorporated");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "KHC",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US5007541064"
            }, "Kraft Heinz Company");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "BABA",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US01609W1027"
            }, "Alibaba");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "ROSN",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU000A0J2Q06"
            }, "Роснефть");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "VZ",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US92343V1044"
            }, "Verizon Communications Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "GMKN",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "RU0007288411"
            }, "ПАО ГМК \"Норильский никель\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "LKOH",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU0009024277"
            }, "ПАО \"ЛУКОЙЛ\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "PHOR",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU000A0JRKT8"
            }, "ПАО \"ФосАгро\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "CSCO",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US17275R1023"
            }, "Cisco Systems");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "TCSG",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US87238U2033"
            }, "TCS Group Holding");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "DIS",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US2546871060"
            }, "The Walt Disney Company");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "ALRS",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU0007252813"
            }, "Акционерная компания \"АЛРОСА\" (публичное акционерное общество)");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "F",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US3453708600"
            }, "Ford Motor Company");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "MU",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US5951121038"
            }, "Micron Technology, Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "TWTR",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US90184L1026"
            }, "Twitter, Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "FB",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US30303M1027"
            }, "Facebook (Meta), Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "BBY",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US0865161014"
            }, "Best Buy Co., Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "CMI",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US2310211063"
            }, "Cummins Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "DOV",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US260003108"
            }, "Dover Corp.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "DOV",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US2600031080"
            }, "Dover Corp.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "LUV",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US8447411088"
            }, "Southwest Airlines Co.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "BIIB",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US09062X1037"
            }, "Biogen Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "ORCL",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US68389X1054"
            }, "Oracle Corporation");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "CCL",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "PA1436583006"
            }, "Carnival Corporation");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "T",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US00206R1023"
            }, "AT&T Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "NVDA",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US67066G1040"
            }, "NVIDIA Corporation");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "VZ",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US92343V104"
            }, "Verizon Communications Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "MRKV",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU000A0JPPN4"
            }, "ПАО \"Россети Волги\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "ADBE",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US00724F1012"
            }, "Adobe Systems Incorporated");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "MSFT",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US5949181045"
            }, "Microsoft Corporation");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "MSNG",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU0008958863"
            }, "ПАО \"Мосэнерго\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "SNGS",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU0008926258"
            }, "ПАО \"Сургутнефтегаз\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "NFLX",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US64110L1061"
            }, "Netflix, Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "BA",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US0970231058"
            }, "The Boeing Company");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "RCL",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "LR0008862868"
            }, "Royal Caribbean Cruises Ltd.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "ENRU",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU000A0F5UN3"
            }, "ПАО \"Энел Россия\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "VTBR",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU000A0JP5V6"
            }, "ПАО \"Банк ВТБ\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "ISKJ",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU000A0JNAB6"
            }, "ПАО \"Институт Стволовых Клеток Человека\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "PANW",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US6974351057"
            }, "Palo Alto Networks, Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "ILMN",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US4523271090"
            }, "Illumina, Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "ALGN",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US0162551016"
            }, "Align Technology, Inc.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "VRTX",
                ExchangeId = (byte)Exchanges.Spb,
                Isin = "US92532F1003"
            }, "Vertex Pharmaceuticals Incorporated");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "YNDX",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "NL0009805522"
            }, "Public Limited Liability Company Yandex N.V.");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "IRKT",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU0006752979"
            }, "ПАО \"Научно - производственная корпорация \"Иркут\"\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "TUZA",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU000A0HL7A2"
            }, "ПАО \"Туймазинский завод автобетоновозов\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "HYDR",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU000A0JPKH7"
            }, "ПАО \"Федеральная гидрогенерирующая компания - РусГидро\"");
            await stockRepository.CreateAsync(new Stock
            {
                CompanyId = "LNZL",
                ExchangeId = (byte)Exchanges.Moex,
                Isin = "RU000A0JP1N2"
            }, "ПАО \"Лензолото\"");


            isins = await stockRepository.GetSampleAsync(x => x.Isin);
        }

        try
        {
            var table = GetExcelTable(file);

            var parser = new ReportParser(table, accounts, isins);

            var reports = await reportRepository.GetSampleAsync(x => x.AccountId == parser.AccountId && x.DateStart <= parser.DateEnd);

            var alreadyDates = reports.SelectMany(x =>
            {
                var _date = x.DateStart;
                var _days = x.DateEnd.DayNumber - x.DateStart.DayNumber;
                var _dates = new List<DateOnly>(_days) { _date };
                while (_days > 0)
                {
                    _date = _date.AddDays(1);
                    _dates.Add(_date);
                    _days--;
                }
                return _dates;
            }).Distinct();

            var transactions = parser.GetTransactions()
                .Where(x => !alreadyDates.Contains(DateOnly.FromDateTime(x.DateTime)))
                .ToList();

            if (!transactions.Any())
            {
                logger.LogInformation(LogEvents.Processing, "Place: {place}. New transactions was not found.", nameof(BcsGrabber) + '.' + file.Name);
                return;
            }

            foreach (var transaction in transactions)
                transaction.DateTime = transaction.DateTime.ToUniversalTime();

            var (transactionsError, createdTransactions) = await transactionRepository.CreateAsync(transactions, new TransactionComparer(), nameof(BcsGrabber));

            if (transactionsError is null)
            {
                var (reportError, _) = await reportRepository.CreateUpdateAsync(new object[] { parser.AccountId, file.Name }, new Report
                {
                    AccountId = parser.AccountId,
                    DateStart = parser.DateStart,
                    DateEnd = parser.DateEnd,
                    Name = file.Name,
                    ContentType = file.ContentType,
                    Payload = file.Payload
                }, file.Name);

                if (reportError is not null)
                    await transactionRepository.DeleteAsync(createdTransactions, "Rollback transactions");
            }
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(BcsGrabber) + '.' + file.Name, exception.Message);

            // automatically creating account
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

internal sealed class ReportParser
{
    private readonly IFormatProvider culture;

    private readonly Dictionary<string, int> reportStructurePatterns;
    private readonly Dictionary<string, Action<Currencies, string>> reportActionPatterns;
    private readonly Dictionary<string, byte> transactionActions;

    private readonly string[] isins;
    private readonly List<Transaction> transactions;

    private readonly DataTable table;
    private int rowId;

    internal ReportParser(DataTable table, IEnumerable<Account> accounts, IEnumerable<string> isins)
    {
        this.table = table;
        culture = new CultureInfo("ru-RU");
        transactions = new List<Transaction>(table.Rows.Count);

        reportStructurePatterns = new Dictionary<string, int>(DocumentPoints.Length);

        while (!TryCellValue(1, "Дата составления отчета:", 1, out var documentPointValue))
            if (documentPointValue is not null && DocumentPoints.Select(x => documentPointValue.IndexOf(x, StringComparison.OrdinalIgnoreCase)).Any(x => x > -1))
                reportStructurePatterns.Add(documentPointValue, rowId);

        if (!reportStructurePatterns.Any())
            throw new Exception("Report structure was not recognized");

        rowId = 0;
        string? _period;
        while (!TryCellValue(1, "Период:", 5, out _period))
            continue;

        if (_period is null)
            throw new Exception($"Agreement period '{_period}' was not recognized");

        var dates = _period.Split(' ');
        DateStart = DateOnly.Parse(dates[1], culture);
        DateEnd = DateOnly.Parse(dates[3], culture);

        string? _agreement;
        while (!TryCellValue(1, "Генеральное соглашение:", 5, out _agreement))
            continue;

        var accountsDictionary = accounts.ToDictionary(x => x.Name, y => y.Id, StringComparer.OrdinalIgnoreCase);
        AccountId = _agreement is not null && accountsDictionary.ContainsKey(_agreement)
                    ? accountsDictionary[_agreement]
                    : throw new Exception($"Agreement '{_agreement}' was not recognized");

        this.isins = isins.ToArray();

        reportActionPatterns = new(StringComparer.OrdinalIgnoreCase)
        {
            { Actions[0], ParseDividend },
            { Actions[1], ParseComission },
            { Actions[2], ParseComission },
            { Actions[3], ParseComission },
            { Actions[4], ParseComission },
            { Actions[5], ParseComission },
            { Actions[6], CheckComission },
            { Actions[7], ParseAccountBalance },
            { Actions[8], ParseAccountBalance },
            { Actions[9], ParseStockTransactions },
            { Actions[10], ParseExchangeRate },
            { Actions[11], ParseComission },
            { Actions[12], ParseComission },
            { Actions[13], ParseComission },
            { Actions[14], ParseComission },
            { Actions[15], ParseComission }
        };
        transactionActions = new()
        {
            { Actions[1], (byte)TransactionActions.Комиссия_брокера },
            { Actions[2], (byte)TransactionActions.Комиссия_брокера },
            { Actions[3], (byte)TransactionActions.Комиссия_депозитария },
            { Actions[4], (byte)TransactionActions.Комиссия_депозитария },
            { Actions[5], (byte)TransactionActions.НДФЛ },
            { Actions[7], (byte)TransactionActions.Пополнение_счета },
            { Actions[8], (byte)TransactionActions.Вывод_с_счета },
            { Actions[11], (byte)TransactionActions.Комиссия_брокера },
            { Actions[12], (byte)TransactionActions.Комиссия_брокера },
            { Actions[13], (byte)TransactionActions.Комиссия_брокера },
            { Actions[14], (byte)TransactionActions.Комиссия_брокера },
            { Actions[15], (byte)TransactionActions.Комиссия_брокера }
        };
    }

    internal int AccountId { get; }
    internal DateOnly DateStart { get; }
    internal DateOnly DateEnd { get; }

    internal List<Transaction> GetTransactions()
    {
        string? cellValue;

        var firstBlock = reportStructurePatterns.Keys.FirstOrDefault(x => x.IndexOf(DocumentPoints[0], StringComparison.OrdinalIgnoreCase) > -1);
        if (firstBlock is not null)
        {
            rowId = reportStructurePatterns[firstBlock];

            var border = reportStructurePatterns.Skip(1).First().Key;

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

        var secondBlock = reportStructurePatterns.Keys.FirstOrDefault(x => x.IndexOf(DocumentPoints[2], StringComparison.OrdinalIgnoreCase) > -1);
        if (secondBlock is not null)
        {
            rowId = reportStructurePatterns[secondBlock] + 3;

            while (!TryCellValue(1, "Итого по валюте Рубль:", 1, out cellValue))
                if (cellValue is not null)
                    reportActionPatterns[DocumentPoints[2]](Currencies.Default, cellValue);
        }

        var thirdBlock = reportStructurePatterns.Keys.FirstOrDefault(x => x.IndexOf(DocumentPoints[3], StringComparison.OrdinalIgnoreCase) > -1);
        if (thirdBlock is not null)
        {
            rowId = reportStructurePatterns[thirdBlock];
            var borders = reportStructurePatterns.Keys
                .Where(x => DocumentPoints[4].IndexOf(x, StringComparison.OrdinalIgnoreCase) > -1 || DocumentPoints[5].IndexOf(x, StringComparison.OrdinalIgnoreCase) > -1)
                .ToArray();

            while (!TryCellValue(1, borders, 6, out cellValue))
                if (cellValue is not null && reportActionPatterns.ContainsKey(cellValue))
                    reportActionPatterns[cellValue](Currencies.Default, cellValue);
        }

        return transactions;
    }

    private void ParseDividend(Currencies currency, string value)
    {
        var info = GetCellValue(14);

        if (info is null)
            throw new Exception(nameof(ParseDividend) + ".Info about dividend was not found");

        var infoArray = info.Split(',').Select(x => x.Trim());

        var isin = isins.Intersect(infoArray).FirstOrDefault();

        if (isin is null || !isins.Contains(isin))
            throw new Exception(nameof(ParseDividend) + $".Isin from '{info}' was not found");

        decimal tax = 0;
        var taxPosition = info.IndexOf("налог", StringComparison.OrdinalIgnoreCase);
        if (taxPosition > -1)
        {
            var taxRow = info[taxPosition..].Split(' ')[1];
            taxRow = taxRow.IndexOf('$') > -1 ? taxRow[1..] : taxRow;
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
    private void ParseComission(Currencies currency, string value)
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
    private void CheckComission(Currencies currency, string value)
    {
        if (!reportActionPatterns.ContainsKey(value))
            throw new Exception(nameof(CheckComission) + $".Comission: '{value}' was not found");
    }
    private void ParseAccountBalance(Currencies currency, string value)
    {
        var costRowIndex = transactionActions[value] switch
        {
            (byte)TransactionActions.Пополнение_счета => 6,
            (byte)TransactionActions.Вывод_с_счета => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(ParseAccountBalance) + ".Transaction type was not recognized")
        };

        transactions.Add(new Transaction
        {
            AccountId = AccountId,
            CurrencyId = (byte)currency,
            TransactionActionId = transactionActions[value],
            DateTime = DateTime.Parse(GetCellValue(1)!, culture),
            Cost = decimal.Parse(GetCellValue(costRowIndex)!),
            Value = 1,
            Info = value
        });
    }
    private void ParseExchangeRate(Currencies currency, string value)
    {
        var name = GetCellValue(1);

        while (!TryCellValue(1, $"Итого по {name}:", 5, out var cellBuyValue))
        {
            var date = DateTime.Parse(GetCellValue(1)!, culture);

            if (!string.IsNullOrWhiteSpace(cellBuyValue))
                transactions.Add(new Transaction
                {
                    AccountId = AccountId,
                    CurrencyId = (byte)Currencies.Rub,
                    TransactionActionId = (byte)TransactionActions.Покупка_валюты,
                    DateTime = date,
                    Cost = decimal.Parse(GetCellValue(4)!),
                    Value = decimal.Parse(cellBuyValue),
                    Info = name
                });
            else
                transactions.Add(new Transaction
                {
                    AccountId = AccountId,
                    CurrencyId = (byte)Currencies.Usd,
                    TransactionActionId = (byte)TransactionActions.Продажа_валюты,
                    DateTime = date,
                    Cost = decimal.Parse(GetCellValue(7)!),
                    Value = decimal.Parse(GetCellValue(8)!),
                    Info = name
                });
        }
    }
    private void ParseStockTransactions(Currencies currency, string value)
    {
        var name = GetCellValue(1);
        var isin = GetCellValue(7);

        if (isin is null)
            throw new Exception(nameof(ParseStockTransactions) + ".Isin from transaction was not found");

        var infoArray = isin.Split(',').Select(x => x.Trim());

        var _isin = isins.Intersect(infoArray).FirstOrDefault();

        if (_isin is null || !isins.Contains(_isin))
            throw new Exception(nameof(ParseStockTransactions) + $".Isin '{isin}' from transaction was not found");

        while (!TryCellValue(1, $"Итого по {name}:", 4, out var cellBuyValue))
        {
            var date = DateTime.Parse(GetCellValue(1)!, culture);
            currency = GetCellValue(10) switch
            {
                "USD" => Currencies.Usd,
                "Рубль" => Currencies.Rub,
                _ => throw new ArgumentOutOfRangeException(nameof(ParseStockTransactions) + '.' + " Currency of transaction was not found")
            };

            if (!string.IsNullOrWhiteSpace(cellBuyValue))
                transactions.Add(new Transaction
                {
                    AccountId = AccountId,
                    StockIsin = isin,
                    CurrencyId = (byte)currency,
                    TransactionActionId = (byte)TransactionActions.Покупка_акции,
                    DateTime = date,
                    Cost = decimal.Parse(GetCellValue(5)!),
                    Value = decimal.Parse(cellBuyValue),
                    Info = name
                });
            else
                transactions.Add(new Transaction
                {
                    AccountId = AccountId,
                    StockIsin = isin,
                    CurrencyId = (byte)currency,
                    TransactionActionId = (byte)TransactionActions.Продажа_акции,
                    DateTime = date,
                    Cost = decimal.Parse(GetCellValue(8)!),
                    Value = decimal.Parse(GetCellValue(7)!),
                    Info = name
                });
        }
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
internal static class ReportConfiguration
{
    internal static readonly string[] DocumentPoints = {
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
        DocumentPoints[2],
        "Приход ДС",
        "Вывод ДС",
        "ISIN:",
        "Сопряж. валюта:",
        "Вознаграждение компании (СВОП)",
        "Комиссия за займы \"овернайт ЦБ\"",
        "Вознаграждение компании (репо)",
        "Комиссия Биржевой гуру",
        "Оплата за вывод денежных средств"
    };
}