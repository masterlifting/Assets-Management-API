using IM.Services.Companies.Reports.Api.Clients;
using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.Services.ReportServices.Implementations;
using IM.Services.Companies.Reports.Api.Services.ReportServices.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Services.Companies.Reports.Api.Enums;

namespace IM.Services.Companies.Reports.Api.Services.ReportServices
{
    public class ReportParser
    {
        private readonly Dictionary<ReportSourceTypes, IReportParser> parser;
        public ReportParser(InvestingClient investingClient) => parser = new()
        {
            { ReportSourceTypes.Investing, new InvestingParser(investingClient) }
        };

        public async Task<Report[]> GetReportsAsync(Ticker ticker) =>
            Enum.TryParse(ticker.SourceTypeId.ToString(), out ReportSourceTypes sourceType) && parser.ContainsKey(sourceType)
            ? await parser[sourceType].GetReportsAsync(ticker)
            : Array.Empty<Report>();
    }
}
