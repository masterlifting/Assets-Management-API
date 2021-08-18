using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.ReportServices.Interfaces
{
    public interface IReportParser
    {
        Task<Report[]> GetReportsAsync(ReportSource sources);
    }
}