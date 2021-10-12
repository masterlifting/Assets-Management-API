using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using IM.Service.Company.Reports.Clients;
using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.Models;
using IM.Service.Company.Reports.Services.ReportServices.Implementations;
using IM.Service.Company.Reports.Services.ReportServices.Interfaces;

using static IM.Service.Company.Reports.Enums;

namespace IM.Service.Company.Reports.Services.ReportServices
{
    public class ReportParser
    {
        private readonly Dictionary<string, IReportParser> parser;
        public ReportParser(InvestingClient investingClient) =>
            parser = new(StringComparer.InvariantCultureIgnoreCase)
            {
                { nameof(ReportSourceTypes.Investing), new InvestingParser(investingClient) }
            };

        public async Task<Report[]> GetReportsAsync(string source, ReportLoaderData data) =>
            parser.ContainsKey(source)
                ? await parser[source].GetReportsAsync(source, data)
                : Array.Empty<Report>();

        public async Task<Report[]> GetReportsAsync(string source, IEnumerable<ReportLoaderData> data) =>
            parser.ContainsKey(source)
                ? await parser[source].GetReportsAsync(source, data)
                : Array.Empty<Report>();
    }
}
