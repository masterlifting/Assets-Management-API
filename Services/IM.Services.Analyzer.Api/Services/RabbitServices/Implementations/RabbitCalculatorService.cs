using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.RabbitServices;
using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;

using System.Threading.Tasks;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.RabbitServices.Implementations
{
    public class RabbitCalculatorService : IRabbitActionService
    {
        private readonly EntityRepository<Report, AnalyzerContext> reportRepository;
        private readonly EntityRepository<Price, AnalyzerContext> priceRepository;

        public RabbitCalculatorService(EntityRepository<Report, AnalyzerContext> reportRepository, EntityRepository<Price, AnalyzerContext> priceRepository)
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

        public async Task<bool> SetReportToCalculateAsync(string data)
        {
            return (RabbitHelper.TrySerialize(data, out AnalyzerReportDto? report) || report is not null)
            && await reportRepository.CreateOrUpdateAsync(new Report
            {
                TickerName = report!.TickerName,
                Year = report.Year,
                Quarter = report.Quarter,
                SourceTypeId = report.SourceTypeId,
                StatusId = (byte)StatusType.ToCalculate
            }, $"analyzer report for {report.TickerName}");
        }
        public async Task<bool> SetPriceToCalculateAsync(string data)
        {
            return (RabbitHelper.TrySerialize(data, out AnalyzerPriceDto? price) || price is not null)
            && await priceRepository.CreateOrUpdateAsync(new Price
            {
                TickerName = price!.TickerName,
                Date = price.Date,
                SourceTypeId = price.SourceTypeId,
                StatusId = (byte)StatusType.ToCalculate
            }, $"analyzer price for {price.TickerName}");
        }
    }
}
