using Microsoft.Extensions.Diagnostics.HealthChecks;

using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;


namespace IM.Service.Common.Net.HttpServices;

public class ExternalEndpointHealthCheck : IHealthCheck
{
    private readonly string host;
    protected ExternalEndpointHealthCheck(string host) => this.host = host;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Ping ping = new();
        var reply = await ping.SendPingAsync(host).ConfigureAwait(false);

        return reply.Status != IPStatus.Success ? HealthCheckResult.Unhealthy() : HealthCheckResult.Healthy();
    }
}