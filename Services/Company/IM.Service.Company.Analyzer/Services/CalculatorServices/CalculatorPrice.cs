using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.Models.Calculator;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using System.Collections.Generic;
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

    public async Task<AnalyzedEntity[]> ComputeAsync(IEnumerable<AnalyzedEntity> data)
    {
        var companies = data
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key);

        List<Task<ResponseModel<PaginatedModel<PriceGetDto>>>> tasks = new(companies.Count);

        foreach (var (key, group) in companies)
        {
            var date = group.OrderBy(x => x.Date).First().Date;

            tasks.Add(client.Get<PriceGetDto>(
                "prices"
                , GetQueryString(HttpRequestFilterType.More, key, date.Year, date.Month, date.Day),
                new(1, int.MaxValue),
                true));
        }

        var results = await Task.WhenAll(tasks);

        return results
            .Where(x => !x.Errors.Any())
            .SelectMany(x => GetComparedSample(x.Data!.Items))
            .ToArray();
    }
    private static IEnumerable<AnalyzedEntity> GetComparedSample(IEnumerable<PriceGetDto> dto) =>
        CalculatorService.CompareSample(dto
            .OrderBy(x => x.Date)
            .Select(x => new Sample
            {
                CompanyId = x.Ticker,
                Date = x.Date,
                CompareTypes = CompareTypes.Asc,
                Value = x.ValueTrue
            }))
            .Select(x => new AnalyzedEntity
            {
                CompanyId = x.CompanyId,
                Date = x.Date,
                AnalyzedEntityTypeId = (byte)EntityTypes.Price,
                Result = x.Value
            });
}