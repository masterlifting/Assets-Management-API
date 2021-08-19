
using CommonServices.RabbitServices;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.RabbitServices.Implementations
{
    public class RabbitCalculatorService : IRabbitActionService
    {
        public Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope)
        {
            return Task.FromResult(true);
        }
    }
}
