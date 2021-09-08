using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.RabbitServices;

using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.DataAccess.Repository;

using System.Linq;
using System.Threading.Tasks;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.RabbitServices.Implementations
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
            action == QueueActions.calculate && entity switch
            {
                QueueEntities.report => await SetReportToCalculateAsync(data),
                QueueEntities.price => await SetPriceToCalculateAsync(data),
                _ => true
            };

        public async Task<bool> SetReportToCalculateAsync(string data) =>
            (RabbitHelper.TrySerialize(data, out AnalyzerReportDto? report) || report is not null)
            && !(await reportRepository.CreateUpdateAsync(new Report
            {
                TickerName = report!.TickerName,
                Year = report.Year,
                Quarter = report.Quarter,
                SourceType = report.SourceType,
                StatusId = (byte)StatusType.ToCalculate
            }, report.TickerName)).Any();
        
        public async Task<bool> SetPriceToCalculateAsync(string data) => 
            (RabbitHelper.TrySerialize(data, out AnalyzerPriceDto? price) || price is not null)
            && !(await priceRepository.CreateUpdateAsync(new Price
            {
                TickerName = price!.TickerName,
                Date = price.Date,
                SourceType = price.SourceType,
                StatusId = (byte)StatusType.ToCalculate
            }, price.TickerName)).Any();
    }
}
