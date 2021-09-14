using System.Threading.Tasks;
using IM.Service.Company.Reports.DataAccess.Entities;

namespace IM.Service.Company.Reports.Services.ReportServices.Interfaces
{
    public interface IReportParser
    {
        Task<Report[]> GetReportsAsync(Ticker ticker);
    }
}