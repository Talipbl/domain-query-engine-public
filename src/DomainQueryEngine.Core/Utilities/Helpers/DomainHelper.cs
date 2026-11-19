using Nager.PublicSuffix;
using Nager.PublicSuffix.RuleProviders;

namespace DomainQueryEngine.Core.Utilities
{
    /// <summary>
    /// Provides helper methods for domain name processing.
    /// </summary>
    public static class DomainHelper
    {
        /// <summary>
        /// The public suffix list is downloaded once, on first use, and the parser is reused for
        /// every call afterwards. <see cref="LazyThreadSafetyMode.PublicationOnly"/> means a failed
        /// download is not cached, so the next call retries instead of failing forever.
        /// </summary>
        private static readonly Lazy<DomainParser> Parser = new(CreateParser, LazyThreadSafetyMode.PublicationOnly);

        private static DomainParser CreateParser()
        {
            var ruleProvider = new SimpleHttpRuleProvider();
            ruleProvider.BuildAsync().GetAwaiter().GetResult();
            return new DomainParser(ruleProvider);
        }

        /// <summary>
        /// Gets the top-level domain (TLD) for the specified domain or host name,
        /// for example "co.uk" or "com", using the public suffix list.
        /// </summary>
        public static string GetTldFromDomainName(string domainName)
            => Parser.Value.Parse(domainName)?.TopLevelDomain ?? string.Empty;

        /// <summary>
        /// Gets the registered domain label for the specified domain or host name
        /// (for example "example" for "www.example.co.uk").
        /// </summary>
        public static string GetDomainFromDomainName(string domainName)
            => Parser.Value.Parse(domainName)?.Domain ?? string.Empty;

        public static string ValidateAndNormalizeDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentException("Domain name cannot be null or empty.", nameof(domain));

            // The UI sends the input with encodeURIComponent, and ASP.NET Core leaves "%2F" encoded
            // in route values, so "https://x.com/a" arrives as "https:%2F%2Fx.com%2Fa".
            domain = Uri.UnescapeDataString(domain);

            domain = SecurityHelper.SanitizeSqlInput(domain);
            domain = SecurityHelper.SanitizeXssInput(domain);

            domain = domain.Trim().ToLowerInvariant();
            if (domain.StartsWith("http://"))
                domain = domain.Substring(7);
            else if (domain.StartsWith("https://"))
                domain = domain.Substring(8);
            // Drop any path, query string or fragment from a pasted URL ("example.com/a?b#c").
            var endOfHost = domain.IndexOfAny(new[] { '/', '\\', '?', '#' });
            if (endOfHost >= 0)
                domain = domain.Substring(0, endOfHost);

            if (domain.StartsWith("www."))
                domain = domain.Substring(4);

            domain = domain.Replace("/", "")
                           .Replace("\\", "")
                           .Replace(" ", "")
                           .Replace("'", "")
                           .Replace("\"", "");

            if (TryGetRegistrableDomain(domain, out var registrable))
                return registrable;

            // Input without a known suffix ("ornek") is searched as ".com".
            if (TryGetRegistrableDomain($"{domain.Replace(".", "")}.com", out registrable))
                return registrable;

            throw new ArgumentException("Invalid domain name format.", nameof(domain));
        }

        private static bool TryGetRegistrableDomain(string domain, out string registrableDomain)
        {
            registrableDomain = string.Empty;
            if (!Parser.Value.TryParse(domain, out DomainInfo? parsed) || string.IsNullOrEmpty(parsed?.RegistrableDomain))
                return false;

            registrableDomain = parsed.RegistrableDomain;
            return true;
        }
    }
}
