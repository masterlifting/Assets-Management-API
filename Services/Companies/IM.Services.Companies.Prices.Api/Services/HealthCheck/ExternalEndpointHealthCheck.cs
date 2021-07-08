using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IM.Services.Companies.Prices.Api.Services.HealthCheck
{
    public class ExternalEndpointHealthCheck : IHealthCheck
    {
        private readonly string host;
        public ExternalEndpointHealthCheck(string host) => this.host = host;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            Ping ping = new();
            var reply = await ping.SendPingAsync(host);

            if (reply.Status != IPStatus.Success)
                return HealthCheckResult.Unhealthy();

            return HealthCheckResult.Healthy();
        }
    }
}