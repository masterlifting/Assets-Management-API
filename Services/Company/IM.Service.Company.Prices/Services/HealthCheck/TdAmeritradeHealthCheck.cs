using CommonServices.HttpServices;

using IM.Service.Company.Prices.Settings;

using Microsoft.Extensions.Options;

namespace IM.Service.Company.Prices.Services.HealthCheck
{
    public class TdAmeritradeHealthCheck : ExternalEndpointHealthCheck
    {
        protected TdAmeritradeHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.TdAmeritrade.Host) { }
    }
}