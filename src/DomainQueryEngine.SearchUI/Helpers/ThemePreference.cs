namespace DomainQueryEngine.SearchUI.Helpers
{
    /// <summary>
    /// The visitor's chosen color theme ("light" or "dark"), kept in a cookie so the server renders the
    /// page in that theme from the start. Without a choice the page follows the system setting.
    /// </summary>
    public static class ThemePreference
    {
        public const string CookieName = "dqe_theme";
        public const string Light = "light";
        public const string Dark = "dark";

        /// <summary>The chosen theme, or null when the visitor has not chosen one.</summary>
        public static string? Get(HttpContext context)
            => context.Request.Cookies[CookieName] switch
            {
                Light => Light,
                Dark => Dark,
                _ => null
            };

        public static string? Set(HttpContext context, string? theme)
        {
            if (theme is not (Light or Dark))
            {
                context.Response.Cookies.Delete(CookieName);
                return null;
            }

            context.Response.Cookies.Append(CookieName, theme, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false, // the page reads it to keep the switch in sync
                IsEssential = true, // a display preference, not tracking
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps
            });
            return theme;
        }
    }
}
