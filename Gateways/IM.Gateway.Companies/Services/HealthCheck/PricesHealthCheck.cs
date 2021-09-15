
using CommonServices.HttpServices;
using IM.Gateway.Companies.Settings;
using Microsoft.Extensions.Options;

namespace IM.Gateway.Companies.Services.HealthCheck
{
    public class PricesHealthCheck : ExternalEndpointHealthCheck
    {
        protected PricesHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.CompanyPrices.Host) { }
    }
}