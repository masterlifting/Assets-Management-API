using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.Background
{
    public interface IReportUpdater
    {
        Task<int> UpdateReportsAsync();
    }
}