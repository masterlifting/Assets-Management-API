using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Company.Reports.Clients;
using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.Services.ReportServices.Implementations;
using IM.Service.Company.Reports.Services.ReportServices.Interfaces;
using static IM.Service.Company.Reports.Enums;

namespace IM.Service.Company.Reports.Services.ReportServices
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
