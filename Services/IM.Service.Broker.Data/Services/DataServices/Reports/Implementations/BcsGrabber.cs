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
    private readonly Repository<Transaction> transactionRepository;
    private readonly Repository<Stock> stockRepository;
    private readonly ILogger<ReportGrabber> logger;

    public BcsGrabber(
        Repository<Report> reportRepository,
        Repository<Transaction> transactionRepository,
        Repository<Stock> stockRepository,
        ILogger<ReportGrabber> logger)
    {
        this.reportRepository = reportRepository;
        this.transactionRepository = transactionRepository;
        this.stockRepository = stockRepository;
        this.logger = logger;
    }

    public async Task GrabDataAsync(ReportFileDto file, IEnumerable<Account> accounts)
    {

        try
        {
            var stocks = stockRepository.GetDbSet().ToArray();

            #region Test data
            //await stockRepository.CreateAsync(new Stock
            //{
            //    CompanyExchange = new CompanyExchange
            //    {
            //        CompanyId = "LMT",
            //        ExchangeId = (byte)Exchanges.Spb
            //    },
            //    Isin = "US5398301094",
            //    Ticker = "LMT"

            //}, "Lockheed Martin Corporation");
            //await stockRepository.CreateAsync(new Stock
            //{
            //    CompanyExchange = new CompanyExchange
            //    {
            //        CompanyId = "DISCK",
            //        ExchangeId = (byte)Exchanges.Spb
            //    },
            //    Isin = "US25470F3029",
            //    Ticker = "DISCK"

            //}, "Discovery, Inc.");
            //await stockRepository.CreateAsync(new Stock
            //{
            //    CompanyExchange = new CompanyExchange
            //    {
            //        CompanyId = "GAZP",
            //        ExchangeId = (byte)Exchanges.Moex
            //    },
            //    Isin = "RU0007661625",
            //    Ticker = "GAZP2"

            //}, "Публичное акционерное общество \"Газпром\"");
            //await stockRepository.CreateAsync(new Stock
            //{
            //    CompanyExchange = new CompanyExchange
            //    {
            //        CompanyId = "NVTK",
            //        ExchangeId = (byte)Exchanges.Moex
            //    },
            //    Isin = "RU000A0DKVS5",
            //    Ticker = "NVTK_02"

            //}, "публичное акционерное общество \"НОВАТЭК\"");
            //await stockRepository.CreateAsync(new Stock
            //{
            //    CompanyExchange = new CompanyExchange
            //    {
            //        CompanyId = "SBER",
            //        ExchangeId = (byte)Exchanges.Moex
            //    },
            //    Isin = "RU0009029540",
            //    Ticker = "SBER"

            //}, "Публичное акционерное общество \"Сбербанк России\"");
            //await stockRepository.CreateAsync(new Stock
            //{
            //    CompanyExchange = new CompanyExchange
            //    {
            //        CompanyId = "UNAC",
            //        ExchangeId = (byte)Exchanges.Moex
            //    },
            //    Isin = "RU000A0JPLZ7",
            //    Ticker = "UNAC_02"

            //}, "Публичное акционерное общество \"Объединенная авиастроительная корпорация\"");
            #endregion

            var (transactions, report) = Parse(file, accounts, stocks);

            var (error, _) = await transactionRepository.CreateUpdateAsync(transactions, new TransactionComparer(), nameof(BcsGrabber));

            if (error is null)
                await reportRepository.CreateAsync(report, report.FileName);
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(BcsGrabber), exception.Message);
        }
    }

    private static (IEnumerable<Transaction> transactions, Report report) Parse(ReportFileDto file, IEnumerable<Account> accounts, IEnumerable<Stock> stocks)
    {
        var table = GetTable(file);
        var parser = new BcsReportParser(table, accounts, stocks);
        var report = new Report
        {
            AccountId = parser.AccountId,
            FileName = file.Name,
            FileContentType = file.ContentType,
            FilePayload = file.Payload
        };

        var stockTransactions = parser.GetStockTransactions();
        var dividends = parser.GetDividends();
        //var accountTransactions = parser.GetAccountTransactions();
        //var comissions = parser.GetComissions();
        //var exchaneRates = parser.GetExchangeRates();

        //return accountTransactions.Concat(stockTransactions).Concat(comissions).Concat(dividends).Concat(exchaneRates);

        return (new List<Transaction>(), report);
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

    private readonly DataTable table;

    private readonly Dictionary<string, int> stocksDictionary;

    private const string column1 = "column1";
    private readonly int rowDealsId;        //Номер строки, с которой начинаются сделки
    private readonly int rowAssetsId;      //Номер строки, с которой начинаются активы

    public BcsReportParser(DataTable table, IEnumerable<Account> accounts, IEnumerable<Stock> stocks)
    {
        this.table = table;

        stocksDictionary = stocks.ToDictionary(x => x.Isin, y => y.Id);
        var accountsDictionary = accounts.ToDictionary(x => x.Name, y => y.Id);

        var agreement = table
            .Select($"{column1} = 'Генеральное соглашение:'")[0]
            .ItemArray
            .Where(x => x is string)
            .ElementAt(1)?
            .ToString();

        AccountId = agreement is not null && accountsDictionary.ContainsKey(agreement)
                    ? accountsDictionary[agreement]
                    : throw new Exception($"Agreement '{agreement}' was not recognized");

        var deals = table.Select($"{column1} = '2.1. Сделки:'").FirstOrDefault();
        rowDealsId = table.Rows.IndexOf(deals);
        var assets = table.Select($"{column1} = '3. Активы:'").FirstOrDefault();
        rowAssetsId = table.Rows.IndexOf(assets);
    }
    public IEnumerable<Transaction> GetDividends()
    {
        var dividends = new List<Transaction>();

        var culture = new CultureInfo("ru-RU");

        for (var i = 0; i < rowDealsId; i++)
        {
            var startCell = table.Rows[i].ItemArray[1]?.ToString();

            if (startCell is null)
                continue;

            switch (startCell)
            {
                case "USD":
                    ParseDividends(i, Currencies.Usd);
                    break;
                case "Рубль":
                    ParseDividends(i, Currencies.Rub);
                    break;
            }
        }

        return dividends;

        void ParseDividends(int startId, Currencies currency)
        {
            startId += 1;

            while (table.Rows[++startId].ItemArray[1]!.ToString()!.IndexOf("Итого по валюте", StringComparison.OrdinalIgnoreCase) == -1)
                if (table.Rows[startId].ItemArray[2]!.ToString()!.IndexOf("Дивиденд", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    var info = table.Rows[startId].ItemArray[14]?.ToString();

                    if (info is null)
                        throw new Exception(nameof(ParseDividends) + '.'+"Info about tax was not found");

                    var dateTime = DateTime.Parse(table.Rows[startId].ItemArray[1]!.ToString()!);

                    var taxPosition = info.IndexOf("налог", StringComparison.OrdinalIgnoreCase);

                    if (taxPosition < 0)
                        throw new Exception(nameof(ParseDividends) + '.' + $"Tax from '{info}' was not recognized");

                    var taxRow = info[taxPosition..].Split(' ')[1][1..];

                    var tax = decimal.Parse(taxRow, NumberStyles.Number, culture);

                    var infoByArray = info.Split(',').Select(x => x.Trim());

                    var isin = stocksDictionary.Keys.Intersect(infoByArray).FirstOrDefault();

                    if (isin is null || !stocksDictionary.ContainsKey(isin))
                        throw new Exception(nameof(ParseDividends) + '.' + $"Isin from '{info}' was not found");

                    var stockId = stocksDictionary[isin];

                    dividends.Add(new Transaction
                    {
                        AccountId = AccountId,
                        CurrencyId = (byte)currency,
                        TransactionActionId = (byte)TransactionActions.Дивиденд,
                        DateTime = dateTime,
                        Cost = decimal.Parse(table.Rows[startId].ItemArray[6]!.ToString()!),
                        Value = 1,
                        Info = info,
                        StockId = stockId
                    });
                    dividends.Add(new Transaction
                    {
                        AccountId = AccountId,
                        CurrencyId = (byte)currency,
                        TransactionActionId = (byte)TransactionActions.Налог_с_дивиденда,
                        DateTime = dateTime,
                        Cost = tax,
                        Value = 1,
                        Info = info,
                        StockId = stockId
                    });
                }
        }
    }
    public IEnumerable<Transaction> GetStockTransactions()
    {
        var stockTransactions = new List<Transaction>();
        var culture = new CultureInfo("ru-RU");

        var rowId = rowDealsId + 1;

        while (rowDealsId > -1 && rowId < rowAssetsId)
        {
            if (table.Rows[rowId].ItemArray[1]!.ToString()!.IndexOf("Незавершенные сделки", StringComparison.OrdinalIgnoreCase) > -1)
                break;

            if (table.Rows[rowId].ItemArray[6]!.ToString()!.IndexOf("ISIN", StringComparison.OrdinalIgnoreCase) > -1)
            {
                var ticker = table.Rows[rowId].ItemArray[1]!.ToString()!;
                var isin = table.Rows[rowId].ItemArray[7]!.ToString()!;

                if (isin is null || !stocksDictionary.ContainsKey(isin))
                    throw new Exception(nameof(GetStockTransactions) + '.' + $"Isin '{isin}' for '{ticker}' was not found");

                var transactionRowId = rowId + 1;

                while (table.Rows[transactionRowId].ItemArray[1]!.ToString()!.IndexOf($"Итого по {ticker}:", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    var transactionRow = table.Rows[transactionRowId];

                    switch (transactionRow.ItemArray[17]!.ToString()!)
                    {
                        case "ММВБ": break;
                        case "СПБ": break;
                        default: continue;
                    }

                    var buy = transactionRow.ItemArray[4];

                    var date = DateTime.Parse(transactionRow.ItemArray[1]!.ToString()!,culture);
                    var identifier = transactionRow.ItemArray[2]!.ToString();
                    var currencyId = transactionRow.ItemArray[10]!.ToString()! switch
                    {
                        "USD" => (byte)Currencies.Usd,
                        "Рубль" => (byte)Currencies.Rub,
                        _ => throw new ArgumentOutOfRangeException(nameof(GetStockTransactions))
                    };
                    TransactionActions action;
                    decimal value;
                    decimal cost;

                    if (buy is not null)
                    {
                        value = decimal.Parse(buy.ToString()!);
                        cost = decimal.Parse(transactionRow.ItemArray[5]!.ToString()!);
                        action = TransactionActions.Покупка_акции;
                    }
                    else
                    {
                        value = decimal.Parse(transactionRow.ItemArray[7]!.ToString()!);
                        cost = decimal.Parse(transactionRow.ItemArray[5]!.ToString()!);
                        action = TransactionActions.Продажа_акции;
                    }

                    stockTransactions.Add(new Transaction
                    {
                        DateTime = date,
                        Identifier = identifier,
                        Cost = cost,
                        Value = value,
                        TransactionActionId = (byte)action,
                        CurrencyId = currencyId,
                        AccountId = AccountId,
                        StockId = stocksDictionary[isin]
                    });


                    transactionRowId++;
                }
            }

            rowId++;
        }

        return stockTransactions;
    }

    //public IEnumerable<Transaction> GetExchangeRates()
    //{
    //    var exchangeRates = new List<Transaction>();

    //    if (startRowId > 0)
    //        for (var i = startRowId + 1; i < endRowId - 1; i++)
    //        {
    //            var rowName = table.Rows[i].ItemArray[1]?.ToString();

    //            if (rowName == "2.3. Незавершенные сделки")
    //                break;

    //            if (rowName != "Иностранная валюта")
    //                continue;

    //            var currency = table.Rows[i + 3].ItemArray[1]?.ToString();
    //            var startId = i + 3;
    //            var finishId = startId;

    //            while (table.Rows[finishId].ItemArray[1]?.ToString() != $"Итого по {currency}:")
    //            {
    //                finishId++;

    //                var typeDeal = table.Rows[++startId].ItemArray[12]?.ToString();
    //                var antiPattern = typeDeal.Split(' ')[0].IndexOf("Своп", StringComparison.OrdinalIgnoreCase);

    //                if (antiPattern >= 0)
    //                    continue;

    //                BuildExchangeRate(startId, 4, 7);
    //            }

    //            var currency2 = table.Rows[++finishId].ItemArray[1]?.ToString();

    //            if (string.IsNullOrWhiteSpace(currency2))
    //                continue;

    //            var startId2 = finishId + 1;
    //            var finishId2 = startId2;

    //            while (table.Rows[finishId2].ItemArray[1]?.ToString() != $"Итого по {currency2}:")
    //            {
    //                finishId2++;

    //                if (table.Rows[++startId2].ItemArray[12]?.ToString() != "РПС")
    //                    continue;

    //                BuildExchangeRate(startId2, 4, 7);
    //            }
    //        }

    //    return exchangeRates;

    //    void BuildExchangeRate(int lineTable, int buyPosition, int sellPosition)
    //    {
    //        var quantityBuy = table.Rows[lineTable].ItemArray[5]?.ToString();
    //        var quantitySell = table.Rows[lineTable].ItemArray[8]?.ToString();
    //        var identifier = table.Rows[lineTable].ItemArray[2]?.ToString();
    //        var dateOperation = table.Rows[lineTable].ItemArray[1]?.ToString();

    //        var buy = int.TryParse(quantityBuy, out _);
    //        var sell = int.TryParse(quantitySell, out _);
    //        var identNo = long.TryParse(identifier, out _);

    //        if (buy && identNo)
    //            exchangeRates.Add(new Transaction
    //            {
    //                DateTime = DateTime.Parse(dateOperation),
    //                Identifier = identifier,
    //                Cost = quantityBuy,
    //                CurrencyId = table.Rows[lineTable].ItemArray[buyPosition].ToString(),
    //                TransactionActionId = statusBuy,
    //                Currency = currencyUsd
    //            });
    //        else if (sell && identNo)
    //            exchangeRates.Add(new Transaction
    //            {
    //                DateTime = DateTime.Parse(dateOperation),
    //                Identifier = identifier,
    //                Cost = quantitySell,
    //                CurrencyId = table.Rows[lineTable].ItemArray[sellPosition].ToString(),
    //                TransactionActionId = statusSell,
    //                Currency = currencyUsd
    //            });

    //    }
    //}
    //public IEnumerable<Transaction> GetAccountTransactions()
    //{
    //    var accountTransactions = new List<Transaction>();

    //    for (int i = 0; i < endRowId; i++)
    //    {
    //        string currency = table.Rows[i].ItemArray[1].ToString();

    //        if (currency.Equals("USD"))
    //            BuildAccountTransaction(i, currency);
    //        if (currency.Equals("Рубль"))
    //            BuildAccountTransaction(i, currency);
    //    }

    //    return accountTransactions;

    //    void BuildAccountTransaction(int lineTable, string currency)
    //    {
    //        int startId = lineTable + 2;
    //        int finishId = startId;

    //        while (table.Rows[finishId].ItemArray[1].ToString() != $"Итого по валюте {currency}:")
    //        {
    //            finishId++;
    //        }
    //        переберем все строки найденого диапазона и добавим найденые позиции
    //        for (int j = startId; j < finishId; j++)
    //        {
    //            string dateOperation = table.Rows[j].ItemArray[1].ToString();

    //            if (table.Rows[j].ItemArray[2].ToString().Equals("Приход ДС"))
    //                accountTransactions.Add(new Transaction
    //                {
    //                    DateTime = DateTime.Parse(dateOperation),
    //                    Cost = table.Rows[j].ItemArray[6].ToString(),
    //                    TransactionActionId = statusReceipt,
    //                    Currency = currency.Equals("USD") ? currencyUsd : currencyRub,
    //                });
    //            if (table.Rows[j].ItemArray[2].ToString().Equals("Вывод ДС"))
    //                accountTransactions.Add(new Transaction
    //                {
    //                    DateTime = DateTime.Parse(dateOperation),
    //                    Cost = table.Rows[j].ItemArray[7].ToString(),
    //                    TransactionActionId = statusWithdraw,
    //                    Currency = currency.Equals("USD") ? currencyUsd : currencyRub
    //                });
    //        }
    //    }
    //}

    //public IEnumerable<Transaction> GetComissions()
    //{
    //    var comissions = new List<Transaction>();

    //    var thisComissions = table.Select($"{columnNumber} = '1.3. Удержанные сборы/штрафы (итоговые суммы):'").FirstOrDefault();
    //    if (thisComissions is null)
    //        thisComissions = table.Select($"{columnNumber} = '1.3. Начисленные сборы/штрафы (итоговые суммы):'").FirstOrDefault();

    //    int startId = table.Rows.IndexOf(thisComissions);

    //    Ищу удержания
    //    if (startId > 0)
    //    {
    //        startId += 4;
    //        int finishId = startId;

    //        while (table.Rows[finishId].ItemArray[1].ToString() != $"Итого по валюте Рубль:")
    //        {
    //            finishId++;
    //        }
    //        Сначала добавим те типы удержаний, которые указаны в разделе Удержания этого отчета
    //        for (int i = startId; i < finishId; i++)
    //        {
    //            string comissionName = table.Rows[i].ItemArray[1].ToString();
    //            string comissionExchangeName = table.Rows[i].ItemArray[8].ToString();

    //            for (int k = 0; k < startId; k++)
    //            {
    //                if (table.Rows[k].ItemArray[2].ToString().Equals(comissionName) && table.Rows[k].ItemArray[12].ToString().Equals(comissionExchangeName))
    //                {
    //                    comissions.Add(new Transaction
    //                    {
    //                        Type = comissionName,
    //                        DateTime = table.Rows[k].ItemArray[1].ToString(),
    //                        Cost = table.Rows[k].ItemArray[7].ToString(),
    //                        Currency = currencyRub
    //                    });
    //                }
    //                else if (table.Rows[k].ItemArray[2].ToString().Equals(comissionName) && table.Rows[k].ItemArray[10].ToString().Equals(comissionExchangeName))
    //                {
    //                    comissions.Add(new Transaction
    //                    {
    //                        Type = comissionName,
    //                        DateTime = table.Rows[k].ItemArray[1].ToString(),
    //                        Cost = table.Rows[k].ItemArray[7].ToString(),
    //                        Currency = currencyRub
    //                    });
    //                }
    //            }
    //        }
    //    }

    //    SetNdfl(comissions);

    //    return comissions;

    //    void SetNdfl(List<Transaction> result)
    //    {
    //        var ndfl = table.Select($"{columnNumber} = '1.1. Движение денежных средств по совершенным сделкам:'").FirstOrDefault();
    //        int ndflStartId = table.Rows.IndexOf(ndfl);
    //        int ndflFinishId = ndflStartId;

    //        for (int i = ndflStartId; i < endRowId; i++)
    //            if (table.Rows[i].ItemArray[1].ToString() == $"Итого по валюте Рубль:")
    //                ndflFinishId = i;

    //        if (ndflStartId == ndflFinishId)
    //            return;

    //        for (int i = ndflStartId; i < ndflFinishId; i++)
    //        {
    //            string ndflName = table.Rows[i].ItemArray[2].ToString();

    //            if (ndflName.Equals("НДФЛ"))
    //            {
    //                result.Add(new Transaction
    //                {
    //                    Type = ndflName,
    //                    DateTime = table.Rows[i].ItemArray[1].ToString(),
    //                    Cost = table.Rows[i].ItemArray[7].ToString(),
    //                    Currency = currencyRub
    //                });
    //            }
    //        }
    //    }
    //}
}