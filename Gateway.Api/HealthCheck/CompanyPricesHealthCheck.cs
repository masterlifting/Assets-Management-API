
using CommonServices.HttpServices;
using Gateway.Api.Settings;
using Microsoft.Extensions.Options;

namespace Gateway.Api.HealthCheck
{
    public class CompanyPricesHealthCheck : ExternalEndpointHealthCheck
    {
        public CompanyPricesHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.CompanyPrices.Host) { }
    }
}