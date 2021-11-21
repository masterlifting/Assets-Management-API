using IM.Service.Common.Net.RabbitServices;
using System;
using System.Threading.Tasks;
using IM.Service.Common.Net;
using IM.Service.Company.Data.Services.DataServices.Prices;
using IM.Service.Company.Data.Services.DataServices.Reports;
using Microsoft.Extensions.Logging;

namespace IM.Service.Company.Data.Services.MqServices.Implementations
{
    public class RabbitFunctionService : IRabbitActionService
    {
        private readonly ILogger<RabbitActionService> logger;
        private readonly PriceLoader priceLoader;
        private readonly ReportLoader reportLoader;

        public RabbitFunctionService(ILogger<RabbitActionService> logger, PriceLoader priceLoader, ReportLoader reportLoader)
        {
            this.logger = logger;
            this.priceLoader = priceLoader;
            this.reportLoader = reportLoader;
        }

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string companyId)
        {
            if (action != QueueActions.Call) 
                return true;
            
            try
            {
                switch (entity)
                {
                    case QueueEntities.Price:
                        await priceLoader.DataSetAsync(companyId);
                        break;
                    case QueueEntities.Report:
                        await reportLoader.DataSetAsync(companyId);
                        break;
                }

                return true;
            }
            catch (Exception exception)
            {
                logger.LogError(LogEvents.Call,"{entity} {action} failed! Error: {error}", nameof(entity), nameof(action), exception.Message);
                return false;
            }
        }
    }
}
