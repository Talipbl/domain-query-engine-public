using DomainQueryEngine.Application.TldPricing;
using Microsoft.Extensions.Options;

namespace DomainQueryEngine.SearchUI.Hosting.TldPricing
{
    /// <summary>Runs scheduled collection in a fresh DI scope so the database context is never shared.</summary>
    public sealed class TldPriceCollectionHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<TldPriceCollectionOptions> _options;
        private readonly ILogger<TldPriceCollectionHostedService> _logger;

        public TldPriceCollectionHostedService(IServiceScopeFactory scopeFactory,
            IOptions<TldPriceCollectionOptions> options, ILogger<TldPriceCollectionHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.Value.IntervalMinutes));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var collector = scope.ServiceProvider.GetRequiredService<TldPriceCollectionService>();
                    await collector.CollectAsync(_options.Value.MaxConcurrency, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception error)
                {
                    _logger.LogError(error, "Scheduled TLD price collection failed");
                }

                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}
