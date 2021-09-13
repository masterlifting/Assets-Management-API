using System.Threading.Tasks;
using IM.Services.Company.Reports.DataAccess.Entities;

namespace IM.Services.Company.Reports.Services.ReportServices.Interfaces
{
    public interface IReportParser
    {
        Task<Report[]> GetReportsAsync(Ticker ticker);
    }
}