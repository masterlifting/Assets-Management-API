using IM.Services.Companies.Prices.Api.Settings;

using Microsoft.Extensions.Options;

namespace IM.Services.Companies.Prices.Api.Services.HealthCheck
{
    public class MoexHealthCheck : ExternalEndpointHealthCheck
    {
        public MoexHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.Moex.Host) { }
    }
}