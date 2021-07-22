using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.Background.ReportUpdaterBackgroundServices.Interfaces
{
    public interface IReportUpdater
    {
        Task<int> UpdateReportsAsync();
    }
}