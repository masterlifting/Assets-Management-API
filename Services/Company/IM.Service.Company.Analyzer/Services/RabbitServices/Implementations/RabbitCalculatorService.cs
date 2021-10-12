using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Dto.CompanyReports;
using CommonServices.RabbitServices;

using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;

using System.Linq;
using System.Threading.Tasks;

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
            action == QueueActions.SetLogic && entity switch
            {
                QueueEntities.Report => await SetReportToCalculateAsync(data),
                QueueEntities.Price => await SetPriceToCalculateAsync(data),
                _ => true
            };

        private async Task<bool> SetReportToCalculateAsync(string data) => RabbitHelper.TrySerialize(data, out ReportGetDto? item) && item is not null
            && !(await reportRepository.CreateUpdateAsync(new Report
            {
                TickerName = item.TickerName,
                SourceType = item.SourceType,
                Year = item.Year,
                Quarter = item.Quarter,
                StatusId = (byte)StatusType.ToCalculate
            }, $"set '{item.TickerName}' for report"))
            .Any();

        private async Task<bool> SetPriceToCalculateAsync(string data) => RabbitHelper.TrySerialize(data, out PriceGetDto? item) && item is not null
            && !(await priceRepository.CreateUpdateAsync(new Price
            {
                TickerName = item.TickerName,
                SourceType = item.SourceType,
                Date = item.Date,
                StatusId = (byte)StatusType.ToCalculate
            }, $"set '{item.TickerName}' for price"))
            .Any();
    }
}
