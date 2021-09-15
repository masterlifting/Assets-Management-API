using Microsoft.Extensions.Diagnostics.HealthChecks;

using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;


namespace CommonServices.HttpServices
{
    public class ExternalEndpointHealthCheck : IHealthCheck
    {
        private readonly string host;
        protected ExternalEndpointHealthCheck(string host) => this.host = host;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            Ping ping = new();
            var reply = await ping.SendPingAsync(host);

            return reply.Status != IPStatus.Success ? HealthCheckResult.Unhealthy() : HealthCheckResult.Healthy();
        }
    }
}
