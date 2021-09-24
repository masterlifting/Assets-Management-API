
using CommonServices.HttpServices;
using Gateway.Api.Settings;
using Microsoft.Extensions.Options;

namespace Gateway.Api.HealthCheck
{
    public class CompaniesHealthCheck : ExternalEndpointHealthCheck
    {
        public CompaniesHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.Companies.Host){}
    }
}