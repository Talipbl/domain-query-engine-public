using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.TldPricing;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DomainQueryEngine.Application.TldPricing
{
    /// <summary>Fetches provider lists independently and atomically replaces only successful snapshots.</summary>
    public sealed class TldPriceCollectionService
    {
        private static readonly Regex TldPattern = new(@"^[a-z0-9-]+(?:\.[a-z0-9-]+)*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CurrencyPattern = new(@"^[A-Z]{3}$", RegexOptions.Compiled);
        private readonly IReadOnlyList<ITldPriceSource> _strategies;
        private readonly ITldPriceRepository _repository;
        private readonly ILogger<TldPriceCollectionService> _logger;

        public TldPriceCollectionService(IEnumerable<ITldPriceSource> strategies, ITldPriceRepository repository,
            ILogger<TldPriceCollectionService> logger)
        {
            _strategies = strategies.ToList();
            _repository = repository;
            _logger = logger;
        }

        public async Task CollectAsync(int maxConcurrency = 3, CancellationToken ct = default)
        {
            if (_strategies.Count == 0) return;
            using var limit = new SemaphoreSlim(Math.Clamp(maxConcurrency, 1, 8));
            var outcomes = await Task.WhenAll(_strategies.Select(async strategy =>
            {
                await limit.WaitAsync(ct);
                try
                {
                    var attempt = DateTime.UtcNow;
                    var snapshot = await strategy.FetchAsync(ct);
                    return new CollectionOutcome(strategy, attempt, Validate(snapshot, strategy), null);
                }
                catch (Exception error) when (error is not OperationCanceledException)
                {
                    return new CollectionOutcome(strategy, DateTime.UtcNow, null, error);
                }
                finally
                {
                    limit.Release();
                }
            }));

            // DbContext is scoped and not thread-safe, so persist outcomes sequentially after parallel fetches.
            foreach (var outcome in outcomes)
            {
                ct.ThrowIfCancellationRequested();
                if (outcome.Error != null)
                {
                    _logger.LogWarning(outcome.Error, "TLD price collection failed for {ProviderSlug} from {SourceUrl}",
                        outcome.Strategy.ProviderSlug, outcome.Strategy.SourceUrl);
                    await _repository.RecordProviderPriceFailureAsync(outcome.Strategy.ProviderSlug,
                        outcome.Strategy.SourceUrl, outcome.AttemptedAt, outcome.Error.Message, ct);
                    continue;
                }

                try
                {
                    await _repository.ReplaceProviderPricesAsync(outcome.Snapshot!, ct);
                    _logger.LogInformation("Imported {PriceCount} TLD prices for {ProviderSlug}",
                        outcome.Snapshot!.Prices.Count, outcome.Strategy.ProviderSlug);
                }
                catch (Exception error) when (error is not OperationCanceledException)
                {
                    _logger.LogError(error, "Could not store the TLD price list for {ProviderSlug}", outcome.Strategy.ProviderSlug);
                    await _repository.RecordProviderPriceFailureAsync(outcome.Strategy.ProviderSlug,
                        outcome.Strategy.SourceUrl, DateTime.UtcNow, error.Message, ct);
                }
            }
        }

        private static TldPriceSnapshot Validate(TldPriceSnapshot snapshot, ITldPriceSource strategy)
        {
            if (!string.Equals(snapshot.ProviderSlug, strategy.ProviderSlug, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The provider slug in the price result does not match its strategy.");
            if (snapshot.Prices.Count == 0)
                throw new InvalidDataException("The provider returned an empty TLD price list.");

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var prices = new List<TldProviderPrice>();
            foreach (var price in snapshot.Prices)
            {
                var tld = price.Tld.Trim().TrimStart('.').ToLowerInvariant();
                var currency = price.Currency.Trim().ToUpperInvariant();
                if (!TldPattern.IsMatch(tld) || !CurrencyPattern.IsMatch(currency))
                    throw new InvalidDataException($"Invalid TLD or currency in {strategy.ProviderSlug}'s price list.");
                if (!seen.Add(tld))
                    throw new InvalidDataException($"The provider returned a duplicate price for '{tld}'.");
                if (price.Registration is <= 0 || price.Renewal is <= 0 || price.RegularRegistration is <= 0)
                    throw new InvalidDataException($"The provider returned a non-positive price for '{tld}'.");
                if (price.Registration == null && price.Renewal == null)
                    throw new InvalidDataException($"The provider returned no registration or renewal price for '{tld}'.");
                if (price.RegistrationTermYears < 1 || price.RenewalTermYears < 1)
                    throw new InvalidDataException($"The provider returned an invalid price term for '{tld}'.");

                prices.Add(new TldProviderPrice
                {
                    Provider = strategy.ProviderSlug,
                    Currency = currency,
                    Tld = tld,
                    Registration = price.Registration,
                    RegularRegistration = price.RegularRegistration is > 0 && price.RegularRegistration > price.Registration
                        ? price.RegularRegistration : null,
                    RegistrationTermYears = price.RegistrationTermYears,
                    Renewal = price.Renewal,
                    RenewalTermYears = price.RenewalTermYears
                });
            }

            return new TldPriceSnapshot
            {
                ProviderSlug = strategy.ProviderSlug,
                SourceUrl = strategy.SourceUrl,
                ObservedAt = snapshot.ObservedAt == default ? DateTime.UtcNow : snapshot.ObservedAt,
                Prices = prices
            };
        }

        private sealed record CollectionOutcome(ITldPriceSource Strategy, DateTime AttemptedAt,
            TldPriceSnapshot? Snapshot, Exception? Error);
    }
}
