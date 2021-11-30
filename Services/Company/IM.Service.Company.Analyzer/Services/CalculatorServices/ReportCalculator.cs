using IM.Service.Common.Net;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Company.Analyzer.Models.Calculator;
using Microsoft.Extensions.DependencyInjection;
using static IM.Service.Common.Net.CommonEnums;
using static IM.Service.Common.Net.HttpServices.QueryStringBuilder;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public class ReportCalculator : IAnalyzerCalculator
{
    private readonly CompanyDataClient client;
    public ReportCalculator(IServiceScopeFactory scopeFactory) => this.client = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<CompanyDataClient>();

    public async Task<RatingData[]> ComputeAsync(IEnumerable<AnalyzedEntity> data)
    {
        var companies = data.GroupBy(x => x.CompanyId).ToDictionary(x => x.Key);
        List<Task<ResponseModel<PaginatedModel<ReportGetDto>>>> tasks = new(companies.Count);

        foreach (var (key, entities) in companies)
        {
            var date = entities.OrderBy(x => x.Date).First().Date;
            var quarter = CommonHelper.QarterHelper.GetQuarter(date.Month);

            tasks.Add(client.Get<ReportGetDto>(
                "reports",
                GetQueryString(HttpRequestFilterType.More, key, date.Year, quarter),
                new(1, int.MaxValue)));
        }

        var results = await Task.WhenAll(tasks);

        return results
            .Where(x => !x.Errors.Any())
            .SelectMany(x => GetComparedSample(x.Data!.Items))
            .ToArray();
    }
    private static IEnumerable<RatingData> GetComparedSample(IEnumerable<ReportGetDto> dto) =>
        dto
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Quarter)
            .Select(x => new Sample[]
            {
                 new () { CompanyId = x.Ticker, CompareTypes = CompareTypes.Asc, Value = x.Revenue.HasValue ? x.Revenue!.Value : 0 }
                ,new () { CompanyId = x.Ticker, CompareTypes = CompareTypes.Asc, Value = x.ProfitNet.HasValue ? x.ProfitNet!.Value : 0 }
                ,new () { CompanyId = x.Ticker, CompareTypes = CompareTypes.Asc, Value = x.ProfitGross.HasValue ? x.ProfitGross!.Value : 0 }
                ,new () { CompanyId = x.Ticker, CompareTypes = CompareTypes.Asc, Value = x.Asset.HasValue ? x.Asset!.Value : 0 }
                ,new () { CompanyId = x.Ticker, CompareTypes = CompareTypes.Asc, Value = x.Turnover.HasValue ? x.Turnover!.Value : 0 }
                ,new () { CompanyId = x.Ticker, CompareTypes = CompareTypes.Asc, Value = x.ShareCapital.HasValue ? x.ShareCapital!.Value : 0 }
                ,new () { CompanyId = x.Ticker, CompareTypes = CompareTypes.Asc, Value = x.CashFlow.HasValue ? x.CashFlow!.Value : 0 }
                ,new () { CompanyId = x.Ticker, CompareTypes = CompareTypes.Desc, Value = x.Obligation.HasValue ? x.Obligation!.Value : 0 }
                ,new () { CompanyId = x.Ticker, CompareTypes = CompareTypes.Desc, Value = x.LongTermDebt.HasValue ? x.LongTermDebt!.Value : 0 }
            })
            .Select(RatingComparator.CompareSample)
            .Select((x, i) => new RatingData
            {
                CompanyId = x[i].CompanyId,
                AnalyzedEntityTypeId = (byte)EntityTypes.Report,
                Result = RatingComparator.ComputeSampleResult(x.Select(y => y.Value).ToArray())
            });
}