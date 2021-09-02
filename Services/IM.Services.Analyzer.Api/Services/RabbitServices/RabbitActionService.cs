using CommonServices.RabbitServices;

using IM.Services.Analyzer.Api.Services.RabbitServices.Implementations;
using IM.Services.Analyzer.Api.Settings;

using Microsoft.Extensions.Options;

namespace IM.Services.Analyzer.Api.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(IOptions<ServiceSettings> options) : base(
            new()
            {
                { QueueExchanges.crud, new RabbitCrudService() },
                { QueueExchanges.calculator, new RabbitCalculatorService() }
            })
        { }
    }
}
