using IM.Service.Common.Net.RabbitServices;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Services.DataServices.Prices;
using IM.Service.Company.Data.Services.DataServices.Reports;
using IM.Service.Company.Data.Services.MqServices.Implementations;
using IM.Service.Company.Data.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Company.Data.Services.MqServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(
            IOptions<ServiceSettings> options,
            ReportLoader reportLoader,
            PriceLoader priceLoader,
            RepositorySet<DataAccess.Entities.Company> companyRepository) : base(
            new()
            {
                { QueueExchanges.Sync, new RabbitSyncService(options.Value.ConnectionStrings.Mq, companyRepository) },
                { QueueExchanges.Function, new RabbitFunctionService(priceLoader, reportLoader) }
            })
        { }
    }
}
