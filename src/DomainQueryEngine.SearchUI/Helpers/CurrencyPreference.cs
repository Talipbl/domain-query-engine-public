using DomainQueryEngine.Application.DomainSearch;

namespace DomainQueryEngine.SearchUI.Helpers
{
    /// <summary>
    /// The visitor's display currency, kept in a cookie so server-rendered pages use it too.
    /// </summary>
    public static class CurrencyPreference
    {
        public const string CookieName = "dqe_currency";

        public static IReadOnlyList<(string Code, string Label)> Options { get; } = new[]
        {
            ("TRY", "₺ TRY"),
            ("USD", "$ USD"),
            ("EUR", "€ EUR")
        };

        /// <summary>The saved currency, or TRY when none (or an unsupported one) is saved.</summary>
        public static string Get(HttpContext context)
            => PriceComparisonService.NormalizeCurrency(context.Request.Cookies[CookieName]);

        public static string Set(HttpContext context, string? currency)
        {
            var normalized = PriceComparisonService.NormalizeCurrency(currency);
            context.Response.Cookies.Append(CookieName, normalized, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false, // the page reads it to keep the switch in sync
                IsEssential = true, // a display preference, not tracking
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps
            });
            return normalized;
        }
    }
}
