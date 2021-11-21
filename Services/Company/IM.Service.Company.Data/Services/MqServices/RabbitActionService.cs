using IM.Service.Common.Net.RabbitServices;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Services.DataServices.Prices;
using IM.Service.Company.Data.Services.DataServices.Reports;
using IM.Service.Company.Data.Services.MqServices.Implementations;

using Microsoft.Extensions.Logging;

namespace IM.Service.Company.Data.Services.MqServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(
            ILogger<RabbitActionService> logger,
            ReportLoader reportLoader,
            PriceLoader priceLoader,
            RepositorySet<DataAccess.Entities.Company> companyRepository,
            RepositorySet<CompanySourceType> cstRepository) : base(
            new()
            {
                { QueueExchanges.Sync, new RabbitSyncService(companyRepository, cstRepository) },
                { QueueExchanges.Function, new RabbitFunctionService(logger, priceLoader, reportLoader) }
            })
        {
        }
    }
}
