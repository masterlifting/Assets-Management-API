using IM.Service.Common.Net.RabbitServices;
using System;
using System.Threading.Tasks;
using IM.Service.Company.Data.Services.DataServices.Prices;
using IM.Service.Company.Data.Services.DataServices.Reports;

namespace IM.Service.Company.Data.Services.MqServices.Implementations
{
    public class RabbitFunctionService : IRabbitActionService
    {
        private readonly PriceLoader priceLoader;
        private readonly ReportLoader reportLoader;

        public RabbitFunctionService(PriceLoader priceLoader, ReportLoader reportLoader)
        {
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{nameof(entity)} {nameof(action)} failed! \nError: {exception.Message}");
                Console.ForegroundColor = ConsoleColor.Gray;

                return false;
            }
        }
    }
}
