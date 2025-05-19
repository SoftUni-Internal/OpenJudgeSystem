namespace OJS.Servers.Infrastructure.Health;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using OJS.Common.Constants;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class LokiHealthCheck : IHealthCheck
{
    private readonly HttpClient client;

    public LokiHealthCheck(IHttpClientFactory clientFactory)
        => this.client = clientFactory.CreateClient(ServiceConstants.OtelCollectorHttpClientName);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        var response = await this.client.GetAsync("-/healthy", cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return response.IsSuccessStatusCode
            ? HealthCheckResult.Healthy(content)
            : HealthCheckResult.Unhealthy(content);
    }
}