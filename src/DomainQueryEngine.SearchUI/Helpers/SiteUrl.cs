namespace DomainQueryEngine.SearchUI.Helpers
{
    /// <summary>
    /// Absolute URLs for canonical links, Open Graph and the sitemap. Uses "Site:BaseUrl" when set (the
    /// public address, e.g. behind a proxy that terminates HTTPS); otherwise the incoming request's host.
    /// </summary>
    public sealed class SiteUrl
    {
        private readonly string? _configured;

        public SiteUrl(IConfiguration configuration)
        {
            var value = configuration["Site:BaseUrl"];
            _configured = string.IsNullOrWhiteSpace(value) ? null : value.TrimEnd('/');
        }

        public string Base(HttpRequest request)
            => _configured ?? $"{request.Scheme}://{request.Host}{request.PathBase}";

        /// <summary>Absolute URL for an app-relative path ("/uzantilar").</summary>
        public string Absolute(HttpRequest request, string path)
            => Base(request) + (path.StartsWith('/') ? path : "/" + path);

        /// <summary>The current page without its query string.</summary>
        public string Canonical(HttpRequest request)
            => Absolute(request, string.IsNullOrEmpty(request.Path.Value) ? "/" : request.Path.Value!);
    }
}
