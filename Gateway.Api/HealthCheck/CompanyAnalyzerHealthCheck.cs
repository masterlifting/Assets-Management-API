using CommonServices.HttpServices;
using Gateway.Api.Settings;
using Microsoft.Extensions.Options;

namespace Gateway.Api.HealthCheck
{
    public class CompanyAnalyzerHealthCheck : ExternalEndpointHealthCheck
    {
        public CompanyAnalyzerHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.CompanyAnalyzer.Host) { }
    }
}