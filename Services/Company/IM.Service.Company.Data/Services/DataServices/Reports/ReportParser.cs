using IM.Service.Company.Data.Clients.Report;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.Reports.Implementations;
using IM.Service.Company.Data.Services.DataServices.Reports.Interfaces;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Company.Data.Enums;

namespace IM.Service.Company.Data.Services.DataServices.Reports;

public class ReportParser
{
    private readonly Dictionary<string, IReportParser> parser;
    public ReportParser(ILogger<ReportParser> logger, InvestingClient investingClient) =>
        parser = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { nameof(SourceTypes.Investing), new InvestingParser(logger, investingClient) }
        };

    public bool IsSource(string source) => parser.ContainsKey(source);

    public async Task<Report[]> GetReportsAsync(string source, QuarterDataConfigModel config) =>
        parser.ContainsKey(source)
            ? await parser[source].GetReportsAsync(source, config)
            : Array.Empty<Report>();
    public async Task<Report[]> GetReportsAsync(string source, IEnumerable<QuarterDataConfigModel> config) =>
        parser.ContainsKey(source)
            ? await parser[source].GetReportsAsync(source, config)
            : Array.Empty<Report>();
}