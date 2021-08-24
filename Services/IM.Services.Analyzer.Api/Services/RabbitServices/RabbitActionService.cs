using CommonServices.RabbitServices;

using IM.Services.Analyzer.Api.Services.RabbitServices.Implementations;

namespace IM.Services.Analyzer.Api.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(RabbitHelper rabbitService) : base(
            new()
            {
                { QueueExchanges.crud, new RabbitCrudService(rabbitService) },
                { QueueExchanges.calculator, new RabbitCalculatorService() }
            })
        { }
    }
}
