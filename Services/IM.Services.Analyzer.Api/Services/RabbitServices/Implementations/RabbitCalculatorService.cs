using CommonServices.Models.AnalyzerService;
using CommonServices.RabbitServices;
using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.RabbitServices.Implementations
{
    public class RabbitCalculatorService : IRabbitActionService
    {
        public RabbitCalculatorService()
        {

        }
        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope) =>
            action == QueueActions.calculate && entity switch
            {
                QueueEntities.report => await SetReportToCalculateAsync(data, scope),
                QueueEntities.price => await SetPriceToCalculateAsync(data, scope),
                _ => true
            };

        public async Task<bool> SetReportToCalculateAsync(string data, IServiceScope scope)
        {
            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<Report, AnalyzerContext>>();

            return repository is not null
            && (RabbitHelper.TrySerialize(data, out AnalyzerReportDto? report) || report is not null)
            && await repository.CreateOrUpdateAsync(new { report!.ReportSourceId, report.Year, report.Quarter }, new Report
            {
                TickerName = report.TickerName,
                ReportSourceId = report.ReportSourceId,
                Year = report.Year,
                Quarter = report.Quarter,
                StatusId = (int)StatusType.ToCalculate
            }, string.Intern($"analyzer report for {report.TickerName}"));
        }
        public async Task<bool> SetPriceToCalculateAsync(string data, IServiceScope scope)
        {
            var repository = scope.ServiceProvider.GetRequiredService<EntityRepository<Price, AnalyzerContext>>();

            return repository is not null
            && (RabbitHelper.TrySerialize(data, out AnalyzerPriceDto? price) || price is not null)
            && await repository.CreateOrUpdateAsync(new { price!.TickerName, price.Date },new Price
            {
                TickerName = price.TickerName,
                Date = price.Date,
                StatusId = (int)StatusType.ToCalculate
            }, string.Intern($"analyzer price for {price.TickerName}"));
        }
    }
}
