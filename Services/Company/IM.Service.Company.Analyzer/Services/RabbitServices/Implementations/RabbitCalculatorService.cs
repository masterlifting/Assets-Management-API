using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.RabbitServices;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.RabbitServices.Implementations
{
    public class RabbitCalculatorService : IRabbitActionService
    {
        private readonly RepositorySet<Report> reportRepository;
        private readonly RepositorySet<Price> priceRepository;

        public RabbitCalculatorService(RepositorySet<Report> reportRepository, RepositorySet<Price> priceRepository)
        {
            this.reportRepository = reportRepository;
            this.priceRepository = priceRepository;
        }
        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) =>
            action == QueueActions.GetLogic && entity switch
            {
                QueueEntities.Report => await SetReportToCalculateAsync(data),
                QueueEntities.Price => await SetPriceToCalculateAsync(data),
                _ => true
            };

        private async Task<bool> SetReportToCalculateAsync(string data) =>
            (RabbitHelper.TrySerialize(data, out CompanyAnalyzerReportDto? report) || report is not null)
            && !(await reportRepository.CreateUpdateAsync(new Report
            {
                TickerName = report!.TickerName,
                Year = report.Year,
                Quarter = report.Quarter,
                SourceType = report.SourceType,
                StatusId = (byte)StatusType.ToCalculate
            }, $"report to calculate for '{report.TickerName}'")).Any();

        private async Task<bool> SetPriceToCalculateAsync(string data) => 
            (RabbitHelper.TrySerialize(data, out CompanyAnalyzerPriceDto? price) || price is not null)
            && !(await priceRepository.CreateUpdateAsync(new Price
            {
                TickerName = price!.TickerName,
                Date = price.Date,
                SourceType = price.SourceType,
                StatusId = (byte)StatusType.ToCalculate
            }, $"price to calculate for '{price.TickerName}'")).Any();
    }
}
