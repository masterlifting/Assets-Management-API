using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IM.Service.Company.Analyzer.Services.HealthCheck
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