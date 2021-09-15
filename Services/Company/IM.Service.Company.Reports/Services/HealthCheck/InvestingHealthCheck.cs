using CommonServices.HttpServices;

using IM.Service.Company.Reports.Settings;

using Microsoft.Extensions.Options;

namespace IM.Service.Company.Reports.Services.HealthCheck
{
    public class InvestingHealthCheck : ExternalEndpointHealthCheck
    {
        protected InvestingHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.Investing.Host) { }
    }
}