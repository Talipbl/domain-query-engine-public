using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace DomainQueryEngine.SearchUI.Helpers
{
    /// <summary>
    /// Site languages and the visitor's choice. The choice is kept in ASP.NET Core's standard culture
    /// cookie, which RequestLocalization reads on every request.
    /// </summary>
    public static class LanguagePreference
    {
        public const string DefaultCulture = "tr-TR";

        /// <summary>Supported cultures: short code shown in the switch, culture name, language name.</summary>
        public static IReadOnlyList<(string Code, string Culture, string Name)> Languages { get; } = new[]
        {
            ("tr", "tr-TR", "Türkçe"),
            ("en", "en-US", "English")
        };

        public static string[] Cultures => Languages.Select(l => l.Culture).ToArray();

        /// <summary>Short code ("tr", "en") of the language the current request is rendered in.</summary>
        public static string Current => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public static void Set(HttpContext context, string? code)
        {
            var language = Languages.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));
            var culture = language.Culture ?? DefaultCulture;

            context.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true, // a display preference, not tracking
                    SameSite = SameSiteMode.Lax,
                    Secure = context.Request.IsHttps
                });
        }
    }
}
