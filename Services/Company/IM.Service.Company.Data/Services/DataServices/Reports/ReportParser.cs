using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Company.Data.Clients.Report;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.Reports.Implementations;
using IM.Service.Company.Data.Services.DataServices.Reports.Interfaces;
using static IM.Service.Company.Data.Enums;

namespace IM.Service.Company.Data.Services.DataServices.Reports
{
    public class ReportParser
    {
        private readonly Dictionary<string, IReportParser> parser;
        public ReportParser(InvestingClient investingClient) =>
            parser = new(StringComparer.InvariantCultureIgnoreCase)
            {
                { nameof(Enums.SourceTypes.Investing), new InvestingParser(investingClient) }
            };

        public async Task<Report[]> GetReportsAsync(string source, ReportDataConfigModel config) =>
            parser.ContainsKey(source)
                ? await parser[source].GetReportsAsync(source, config)
                : Array.Empty<Report>();
        public async Task<Report[]> GetReportsAsync(string source, IEnumerable<ReportDataConfigModel> config) =>
            parser.ContainsKey(source)
                ? await parser[source].GetReportsAsync(source, config)
                : Array.Empty<Report>();
    }
}
