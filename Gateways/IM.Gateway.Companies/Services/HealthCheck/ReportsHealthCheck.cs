
using IM.Gateway.Companies.Settings;
using Microsoft.Extensions.Options;

namespace IM.Gateway.Companies.Services.HealthCheck
{
    public abstract class ReportsHealthCheck : ExternalEndpointHealthCheck
    {
        protected ReportsHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.CompanyReports.Host){}
    }
}