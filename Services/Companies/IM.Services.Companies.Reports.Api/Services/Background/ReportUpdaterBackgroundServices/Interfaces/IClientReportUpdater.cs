using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.Background.ReportUpdaterBackgroundServices.Interfaces
{
    public interface IClientReportUpdater
    {
        Task<Report[]> GetReportsAsync(ReportSource sources);
    }
}