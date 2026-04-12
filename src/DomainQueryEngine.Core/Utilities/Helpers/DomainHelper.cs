
using Nager.PublicSuffix;
using Nager.PublicSuffix.RuleProviders;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace DomainQueryEngine.Core.Utilities
{
    /// <summary>
    /// Provides helper methods for domain name processing.
    /// </summary>
    public static class DomainHelper
    {
        /// <summary>
        /// Gets the top-level domain (TLD) for the specified domain or host name.
        /// </summary>
        /// <param name="domainName">The domain or host name to analyze (for example, "www.example.co.uk" or "example.com").</param>
        /// <returns>
        /// The top-level domain portion of <paramref name="domainName"/> (for example, "co.uk" or "com").
        /// The exact value is determined by the public suffix list used by <c>Nager.PublicSuffix</c>.
        /// </returns>
        /// <remarks>
        /// This method uses <see cref="SimpleHttpRuleProvider"/> to obtain the public suffix list and
        /// constructs a <see cref="DomainParser"/> to parse the supplied domain name. The rule provider
        /// is built synchronously by calling <c>BuildAsync().Wait()</c>, so this method blocks while the
        /// rules are downloaded/initialized.
        ///
        /// Consider caching the created <see cref="DomainParser"/> or the rule provider if this method is
        /// called frequently to avoid repeated network access and parser construction.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="domainName"/> is null.</exception>
        /// <exception cref="System.Exception">
        /// May throw exceptions from the underlying <c>Nager.PublicSuffix</c> library or network errors
        /// when fetching the public suffix list. Exceptions thrown by <c>BuildAsync().Wait()</c> may be
        /// wrapped in an <see cref="System.AggregateException"/>.
        /// </exception>
        public static string GetTldFromDomainName(string domainName)
        {
            var ruleProvider = new SimpleHttpRuleProvider();
            ruleProvider.BuildAsync().Wait();

            var domainParser = new DomainParser(ruleProvider);

            return domainParser.Parse(domainName).TopLevelDomain;
        }

        /// <summary>
        /// Gets the registered domain for the specified domain or host name.
        /// </summary>
        /// <param name="domainName">The domain or host name to analyze (for example, "www.example.co.uk" or "example.com").</param>
        /// <returns>
        /// The registered domain portion of <paramref name="domainName"/> (for example, "example.co.uk" or "example.com").
        public static string GetDomainFromDomainName(string domainName)
        {
            var ruleProvider = new SimpleHttpRuleProvider();
            ruleProvider.BuildAsync().Wait();
            var domainParser = new DomainParser(ruleProvider);
            return domainParser.Parse(domainName).Domain;
        }

        public static string ValidateAndNormalizeDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domain));

            domain = SecurityHelper.SanitizeSqlInput(domain);
            domain = SecurityHelper.SanitizeXssInput(domain);

            domain = domain.Trim().ToLowerInvariant();
            if (domain.StartsWith("http://"))
                domain = domain.Substring(7);
            else if (domain.StartsWith("https://"))
                domain = domain.Substring(8);
            if (domain.StartsWith("www."))
                domain = domain.Substring(4);

            domain = domain.Replace("/", "")
                           .Replace("\\", "")
                           .Replace(" ", "")
                           .Replace("'", "")
                           .Replace("\"", "");

            var ruleProvider = new SimpleHttpRuleProvider();
            ruleProvider.BuildAsync().Wait();
            var domainParser = new DomainParser(ruleProvider);
            try
            {
                DomainInfo parsedDomain;
                domainParser.TryParse(domain, out parsedDomain);
                return parsedDomain?.RegistrableDomain ?? $"{domain.Replace(".","")}.com";
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid domain name format.", nameof(domain), ex);
            }
        }
    }
}
