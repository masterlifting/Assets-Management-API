using IM.Services.Companies.Reports.Api.Clients;
using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.Services.ReportServices.Implementations;
using IM.Services.Companies.Reports.Api.Services.ReportServices.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Services.Companies.Reports.Api.DataAccess.DataEnums;

namespace IM.Services.Companies.Reports.Api.Services.ReportServices
{
    public class ReportParser
    {
        private readonly Dictionary<ReportSourceTypes, IReportParser> parser;
        public ReportParser(InvestingClient investingClient)
        {
            parser = new()
            {
                { ReportSourceTypes.investing, new InvestingParser(investingClient) }
            };
        }
        public async Task<Report[]> GetReportsAsync(ReportSourceTypes sourceType, ReportSource source) => await parser[sourceType].GetReportsAsync(source);
    }
}
