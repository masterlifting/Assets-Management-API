using IM.Services.Company.Prices.Settings;
using Microsoft.Extensions.Options;

namespace IM.Services.Company.Prices.Services.HealthCheck
{
    public class TdAmeritradeHealthCheck : ExternalEndpointHealthCheck
    {
        public TdAmeritradeHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.TdAmeritrade.Host){}
    }
}