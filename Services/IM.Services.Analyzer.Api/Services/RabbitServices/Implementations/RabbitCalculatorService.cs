using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.RabbitServices;

using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.DataAccess.Repository;

using System.Threading.Tasks;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.RabbitServices.Implementations
{
    public class RabbitCalculatorService : IRabbitActionService
    {
        private readonly AnalyzerRepository<Report> reportRepository;
        private readonly AnalyzerRepository<Price> priceRepository;

        public RabbitCalculatorService(AnalyzerRepository<Report> reportRepository, AnalyzerRepository<Price> priceRepository)
        {
            this.reportRepository = reportRepository;
            this.priceRepository = priceRepository;
        }
        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) =>
            action == QueueActions.calculate && entity switch
            {
                QueueEntities.report => await SetReportToCalculateAsync(data),
                QueueEntities.price => await SetPriceToCalculateAsync(data),
                _ => true
            };

        public async Task<bool> SetReportToCalculateAsync(string data) =>
            (RabbitHelper.TrySerialize(data, out AnalyzerReportDto? report) || report is not null)
            && await reportRepository.CreateOrUpdateAsync(new Report
            {
                TickerName = report!.TickerName,
                Year = report.Year,
                Quarter = report.Quarter,
                SourceType = report.SourceType,
                StatusId = (byte)StatusType.ToCalculate
            }, report.TickerName);
        
        public async Task<bool> SetPriceToCalculateAsync(string data) => 
            (RabbitHelper.TrySerialize(data, out AnalyzerPriceDto? price) || price is not null)
            && await priceRepository.CreateOrUpdateAsync(new Price
            {
                TickerName = price!.TickerName,
                Date = price.Date,
                SourceType = price.SourceType,
                StatusId = (byte)StatusType.ToCalculate
            }, price.TickerName);
    }
}
