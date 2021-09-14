using CommonServices.Models.Dto.AnalyzerService;
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
            action == QueueActions.Calculate && entity switch
            {
                QueueEntities.Report => await SetReportToCalculateAsync(data),
                QueueEntities.Price => await SetPriceToCalculateAsync(data),
                _ => true
            };

        private async Task<bool> SetReportToCalculateAsync(string data) =>
            (RabbitHelper.TrySerialize(data, out AnalyzerReportDto? report) || report is not null)
            && !(await reportRepository.CreateUpdateAsync(new Report
            {
                TickerName = report!.TickerName,
                Year = report.Year,
                Quarter = report.Quarter,
                SourceType = report.SourceType,
                StatusId = (byte)Enums.StatusType.ToCalculate
            }, report.TickerName)).Any();

        private async Task<bool> SetPriceToCalculateAsync(string data) => 
            (RabbitHelper.TrySerialize(data, out AnalyzerPriceDto? price) || price is not null)
            && !(await priceRepository.CreateUpdateAsync(new Price
            {
                TickerName = price!.TickerName,
                Date = price.Date,
                SourceType = price.SourceType,
                StatusId = (byte)Enums.StatusType.ToCalculate
            }, price.TickerName)).Any();
    }
}
