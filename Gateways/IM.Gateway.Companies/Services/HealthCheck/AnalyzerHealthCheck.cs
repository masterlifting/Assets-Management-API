using CommonServices.HttpServices;

using IM.Gateway.Companies.Settings;

using Microsoft.Extensions.Options;

namespace IM.Gateway.Companies.Services.HealthCheck
{
    public class AnalyzerHealthCheck : ExternalEndpointHealthCheck
    {
        protected AnalyzerHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.CompanyAnalyzer.Host) { }
    }
}