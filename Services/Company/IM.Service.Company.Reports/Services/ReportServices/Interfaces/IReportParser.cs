using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.Models;

namespace IM.Service.Company.Reports.Services.ReportServices.Interfaces
{
    public interface IReportParser
    {
        Task<Report[]> GetReportsAsync(string source, ReportLoaderData data);
        Task<Report[]> GetReportsAsync(string source, IEnumerable<ReportLoaderData> data);
    }
}