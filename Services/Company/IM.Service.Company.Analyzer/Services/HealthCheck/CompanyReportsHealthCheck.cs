using IM.Service.Company.Analyzer.Settings;

using Microsoft.Extensions.Options;

namespace IM.Service.Company.Analyzer.Services.HealthCheck
{
    public abstract class CompanyReportsHealthCheck : ExternalEndpointHealthCheck
    {
        protected CompanyReportsHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.CompanyReports.Host){}
    }
}