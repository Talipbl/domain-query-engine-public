using DomainQueryEngine.PostgreSQL;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DomainQueryEngine.SearchUI.Health
{
    public sealed class DatabaseHealthCheck(AppDbContext db) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await db.Database.CanConnectAsync(cancellationToken)
                    ? HealthCheckResult.Healthy("Database connection is available.")
                    : HealthCheckResult.Unhealthy("Database connection is unavailable.");
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                return HealthCheckResult.Unhealthy("Database connection check failed.", error);
            }
        }
    }
}
