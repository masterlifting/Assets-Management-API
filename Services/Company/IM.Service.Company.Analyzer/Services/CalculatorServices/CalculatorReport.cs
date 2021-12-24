using IM.Service.Common.Net;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.Models.Calculator;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Common.Net.CommonEnums;
using static IM.Service.Common.Net.HttpServices.QueryStringBuilder;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public class CalculatorReport : IAnalyzerCalculator
{
    private readonly CompanyDataClient client;
    public CalculatorReport(CompanyDataClient client) => this.client = client;

    public async Task<AnalyzedEntity[]> ComputeAsync(IReadOnlyCollection<AnalyzedEntity> data)
    {
        var _data = data
            .Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Report)
            .ToImmutableArray();

        if (!_data.Any())
            return Array.Empty<AnalyzedEntity>();

        var companyIds = _data
            .Select(x => x.CompanyId)
            .Distinct()
            .ToImmutableArray();

        var date = _data.MinBy(x => x.Date)!.Date.AddYears(-1);

        var quarter = CommonHelper.QarterHelper.GetQuarter(date.Month);

        var response = await client.Get<ReportGetDto>(
            "reports",
            GetQueryString(HttpRequestFilterType.More, companyIds, date.Year, quarter),
            new(1, int.MaxValue),
            true);

        if (!response.Errors.Any())
            return GetComparedSample(response.Data!.Items)
                .ToArray();

        foreach (var item in _data)
            item.StatusId = (byte)Statuses.Error;

        return _data.ToArray();
    }

    private static IEnumerable<AnalyzedEntity> GetComparedSample(in IEnumerable<ReportGetDto> dto) =>
        dto
            .GroupBy(x => x.Ticker)
            .SelectMany(x =>
            {
                var companyOrderedData = x
                    .OrderBy(y => y.Year)
                    .ThenBy(y => y.Quarter)
                    .ToImmutableArray();

                var companySamples = companyOrderedData
                    .Select((y, i) => new Sample[]
                        {
                            new ()  { Id = i, CompareType = CompareTypes.Asc, Value = y.Revenue.HasValue ? y.Revenue!.Value : 0 }
                            ,new () { Id = i, CompareType = CompareTypes.Asc, Value = y.ProfitNet.HasValue ? y.ProfitNet!.Value : 0 }
                            ,new () { Id = i, CompareType = CompareTypes.Asc, Value = y.ProfitGross.HasValue ? y.ProfitGross!.Value : 0 }
                            ,new () { Id = i, CompareType = CompareTypes.Asc, Value = y.Asset.HasValue ? y.Asset!.Value : 0 }
                            ,new () { Id = i, CompareType = CompareTypes.Asc, Value = y.Turnover.HasValue ? y.Turnover!.Value : 0 }
                            ,new () { Id = i, CompareType = CompareTypes.Asc, Value = y.ShareCapital.HasValue ? y.ShareCapital!.Value : 0 }
                            ,new () { Id = i, CompareType = CompareTypes.Asc, Value = y.CashFlow.HasValue ? y.CashFlow!.Value : 0 }
                            ,new () { Id = i, CompareType = CompareTypes.Desc, Value = y.Obligation.HasValue ? y.Obligation!.Value : 0 }
                            ,new () { Id = i, CompareType = CompareTypes.Desc, Value = y.LongTermDebt.HasValue ? y.LongTermDebt!.Value : 0 }
                        })
                    .ToArray();

                var comparedSamples = CalculatorService
                    .CompareSampleByColumn(in companySamples, 9)
                    .SelectMany(y => y)
                    .GroupBy(y => y.Id)
                    .ToImmutableDictionary(y => y.Key);

                return companyOrderedData
                        .Select((report, index) =>
                        {
                            var isComputed = comparedSamples.ContainsKey(index);

                            return new AnalyzedEntity
                            {
                                CompanyId = x.Key,
                                Date = GetDateTime(report.Year, report.Quarter),
                                AnalyzedEntityTypeId = (byte)EntityTypes.Report,
                                StatusId = isComputed
                                    ? (byte)Statuses.Computed
                                    : (byte)Statuses.NotComputed,
                                Result = isComputed
                                    ? CalculatorService.ComputeSampleResult(comparedSamples[index]
                                        .Select(sample => sample.Value)
                                        .ToImmutableArray())
                                    : 0
                            };
                        });

            });

    private static DateTime GetDateTime(int year, byte quarter) =>
        new(year, CommonHelper.QarterHelper.GetLastMonth(quarter), 28);
}