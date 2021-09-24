
using CommonServices.HttpServices;
using Gateway.Api.Settings;
using Microsoft.Extensions.Options;

namespace Gateway.Api.HealthCheck
{
    public class CompanyReportsHealthCheck : ExternalEndpointHealthCheck
    {
        public CompanyReportsHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.CompanyReports.Host){}
    }
}