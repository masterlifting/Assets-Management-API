using System;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.Models.Calculator;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Common.Net.CommonEnums;
using static IM.Service.Common.Net.HttpServices.QueryStringBuilder;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public class CalculatorPrice : IAnalyzerCalculator
{
    private readonly CompanyDataClient client;
    public CalculatorPrice(CompanyDataClient client) => this.client = client;

    public async Task<AnalyzedEntity[]> ComputeAsync(IReadOnlyCollection<AnalyzedEntity> data)
    {
        var _data = data
            .Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Price)
            .ToImmutableArray();

        if (!_data.Any())
            return Array.Empty<AnalyzedEntity>();

        var companyIds = _data
            .Select(x => x.CompanyId)
            .Distinct()
            .ToImmutableArray();

        var date = _data.MinBy(x => x.Date)!.Date.AddDays(-15);

        var response = await client.Get<PriceGetDto>(
            "prices",
            GetQueryString(HttpRequestFilterType.More, companyIds, date.Year, date.Month, date.Day),
            new(1, int.MaxValue),
            true);

        if (!response.Errors.Any())
            return GetComparedSample(response.Data!.Items)
                .ToArray();

        foreach (var item in _data)
            item.StatusId = (byte)Statuses.Error;

        return _data.ToArray();
    }
    
    private static IEnumerable<AnalyzedEntity> GetComparedSample(in IEnumerable<PriceGetDto> dto) =>
         dto
            .GroupBy(x => x.Ticker)
            .SelectMany(x =>
            {
                var companyOrderedData = x
                    .OrderBy(y => y.Date)
                    .ToImmutableArray();

                var companySample = companyOrderedData
                    .Select((y, i) => new Sample { Id = i, CompareType = CompareTypes.Asc, Value = y.ValueTrue });

                var comparedSample = CalculatorService
                    .CompareSample(companySample)
                    .ToImmutableDictionary(y => y.Id, z => z.Value);

                return companyOrderedData
                        .Select((price, index) =>
                        {
                            var isComputed = comparedSample.ContainsKey(index);

                            return new AnalyzedEntity
                            {
                                CompanyId = x.Key,
                                Date = price.Date,
                                AnalyzedEntityTypeId = (byte)EntityTypes.Price,
                                StatusId = isComputed
                                    ? (byte)Statuses.Computed
                                    : (byte)Statuses.NotComputed,
                                Result = isComputed
                                    ? comparedSample[index]
                                    : 0
                            };
                        });
            });
}