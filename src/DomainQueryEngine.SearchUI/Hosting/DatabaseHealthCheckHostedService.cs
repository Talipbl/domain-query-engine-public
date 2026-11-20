using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DomainQueryEngine.SearchUI.Hosting
{
    /// <summary>Checks database connectivity every five minutes while the application is running.</summary>
    public sealed class DatabaseHealthCheckHostedService(
        HealthCheckService healthChecks,
        ILogger<DatabaseHealthCheckHostedService> logger) : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);
            do
            {
                try
                {
                    var report = await healthChecks.CheckHealthAsync(
                        registration => registration.Name == "database", stoppingToken);

                    if (report.Status == HealthStatus.Healthy)
                        logger.LogDebug("Database health check passed.");
                    else
                        logger.LogWarning("Database health check reported {Status}: {Details}",
                            report.Status, string.Join("; ", report.Entries.Values.Select(entry => entry.Description)));
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception error)
                {
                    logger.LogError(error, "Database health check failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
