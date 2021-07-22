using IM.Services.Companies.Prices.Api.Settings;

using Microsoft.Extensions.Options;

namespace IM.Services.Companies.Prices.Api.Services.HealthCheck
{
    public class TdAmeritradeHealthCheck : ExternalEndpointHealthCheck
    {
        public TdAmeritradeHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.TdAmeritrade.Host){}
    }
}