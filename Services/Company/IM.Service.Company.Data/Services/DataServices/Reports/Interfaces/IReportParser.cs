using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;

namespace IM.Service.Company.Data.Services.DataServices.Reports.Interfaces
{
    public interface IReportParser
    {
        Task<Report[]> GetReportsAsync(string source, ReportDataConfigModel config);
        Task<Report[]> GetReportsAsync(string source, IEnumerable<ReportDataConfigModel> config);
    }
}