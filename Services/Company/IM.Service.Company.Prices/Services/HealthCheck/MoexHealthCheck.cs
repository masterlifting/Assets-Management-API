using CommonServices.HttpServices;

using IM.Service.Company.Prices.Settings;

using Microsoft.Extensions.Options;

namespace IM.Service.Company.Prices.Services.HealthCheck
{
    public class MoexHealthCheck : ExternalEndpointHealthCheck
    {
        protected MoexHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.Moex.Host) { }
    }
}