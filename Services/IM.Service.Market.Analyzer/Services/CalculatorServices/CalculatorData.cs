using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Market.Analyzer.Clients;
using IM.Service.Market.Analyzer.DataAccess.Entities;
using static  IM.Service.Market.Analyzer.Enums;
using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helper;
using static IM.Service.Common.Net.HttpServices.QueryStringBuilder;

namespace IM.Service.Market.Analyzer.Services.CalculatorServices;

public class CalculatorData
{
    private readonly CompanyDataClient client;
    private readonly Dictionary<DateOnly, IEnumerable<string>> priceRequestData;
    private readonly Dictionary<DateOnly, IEnumerable<string>> reportRequestData;

    public IEnumerable<ReportGetDto> Reports { get; }
    public IEnumerable<PriceGetDto> Prices { get; }
    public IEnumerable<EntityTypes> Types { get; }

    public CalculatorData(CompanyDataClient client, IEnumerable<AnalyzedEntity> data)
    {
        this.client = client;
        var _data = data.ToArray();

        priceRequestData = new(_data.Length);
        reportRequestData = new(_data.Length);
        
        List<EntityTypes> types = new(Enum.GetValues<EntityTypes>().Length);

        foreach (var item in _data.GroupBy(x => x.Date).OrderBy(x => x.Key))
        {
            var date = item.Key.AddMonths(-6);

            foreach (var entityType in item.GroupBy(x => x.AnalyzedEntityTypeId))
            {
                var companyIds = entityType
                    .Select(x => x.CompanyId)
                    .ToArray();

                switch (entityType.Key)
                {
                    case (byte)EntityTypes.Price:
                        {
                            if (priceRequestData.ContainsKey(date))
                                priceRequestData[date] = priceRequestData[date].Concat(companyIds.Except(priceRequestData[date]));
                            else
                                priceRequestData.Add(date, companyIds);

                            types.Add(EntityTypes.Price);
                        }
                        break;
                    case (byte)EntityTypes.Report:
                        {
                            if (reportRequestData.ContainsKey(date))
                                reportRequestData[date] = reportRequestData[date].Concat(companyIds.Except(reportRequestData[date]));
                            else
                                reportRequestData.Add(date, companyIds);

                            types.Add(EntityTypes.Report);
                        }
                        break;
                    case (byte)EntityTypes.Coefficient:
                        {
                            if (priceRequestData.ContainsKey(date))
                                priceRequestData[date] = priceRequestData[date].Concat(companyIds.Except(priceRequestData[date]));
                            else
                                priceRequestData.Add(date, companyIds);

                            if (reportRequestData.ContainsKey(date))
                                reportRequestData[date] = reportRequestData[date].Concat(companyIds.Except(reportRequestData[date]));
                            else
                                reportRequestData.Add(date, companyIds);

                            types.Add(EntityTypes.Coefficient);
                        }
                        break;
                }
            }
        }

        Types = types;

        var priceTask = GetPricesAsync();
        var reportTask = GetReportsAsync();

        Task.WhenAll(priceTask,reportTask).GetAwaiter().GetResult();

        Prices = priceTask.Result;
        Reports = reportTask.Result;
    }

    private async Task<PriceGetDto[]> GetPricesAsync()
    {
        List<Task<ResponseModel<PaginatedModel<PriceGetDto>>>> priceRequestTasks = new(priceRequestData.Count);

        foreach (var (date, companyIds) in priceRequestData)
        {
            priceRequestTasks.Add(client.Get<PriceGetDto>(
                "prices",
                GetQueryString(HttpRequestFilterType.More, companyIds, date.Year, date.Month, date.Day),
                new(1, int.MaxValue)));
        }

        var result = await Task.WhenAll(priceRequestTasks);

        return result
            .Where(x => !x.Errors.Any() && x.Data is not null && x.Data.Count > 0)
            .SelectMany(x => x.Data!.Items)
            .ToArray();
    }
    private async Task<ReportGetDto[]> GetReportsAsync()
    {
        List<Task<ResponseModel<PaginatedModel<ReportGetDto>>>> reportRequestTasks = new(reportRequestData.Count);

        foreach (var (date, companyIds) in reportRequestData)
        {
            var quarter = QuarterHelper.GetQuarter(date.Month);

            reportRequestTasks.Add(client.Get<ReportGetDto>(
                "reports",
                GetQueryString(HttpRequestFilterType.More, companyIds, date.Year, quarter),
                new(1, int.MaxValue)));
        }

        var result = await Task.WhenAll(reportRequestTasks);

        return result
            .Where(x => !x.Errors.Any() && x.Data is not null && x.Data.Count > 0)
            .SelectMany(x => x.Data!.Items)
            .ToArray();
    }
}