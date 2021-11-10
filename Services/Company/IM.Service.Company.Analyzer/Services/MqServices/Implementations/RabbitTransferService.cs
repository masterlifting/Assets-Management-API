using IM.Service.Common.Net.RabbitServices;

using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using System.Threading.Tasks;
using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.MqServices.Implementations
{
    public class RabbitTransferService : IRabbitActionService
    {
        private readonly RepositorySet<Report> reportRepository;
        private readonly RepositorySet<Price> priceRepository;

        public RabbitTransferService(RepositorySet<Report> reportRepository, RepositorySet<Price> priceRepository)
        {
            this.reportRepository = reportRepository;
            this.priceRepository = priceRepository;
        }
        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) =>
            action == QueueActions.Create && entity switch
            {
                QueueEntities.Report => await SetReportToCalculateAsync(data),
                QueueEntities.Price => await SetPriceToCalculateAsync(data),
                _ => true
            };
        private async Task<bool> SetReportToCalculateAsync(string data)
        {
            if (!RabbitHelper.TrySerialize(data, out ReportIdentityDto? dto))
                return false;

            var entity = new Report
            {
                CompanyId = dto!.CompanyId,
                Year = dto.Year,
                Quarter = dto.Quarter,
                StatusId = (byte)StatusType.ToCalculate
            };

            return (await reportRepository.CreateUpdateAsync(entity,$"report for '{dto.CompanyId}'")).error is not null;
        }
        private async Task<bool> SetPriceToCalculateAsync(string data)
        {
            if (!RabbitHelper.TrySerialize(data, out PriceIdentityDto? dto))
                return false;

            var entity = new Price
            {
                CompanyId = dto!.CompanyId,
                Date = dto.Date,
                StatusId = (byte)StatusType.ToCalculate
            };

            return (await priceRepository.CreateUpdateAsync(entity, $"price for '{dto.CompanyId}'")).error is not null;
        }
    }
}
