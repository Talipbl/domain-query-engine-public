using DomainQueryEngine.SearchUI.Models;
using DomainQueryEngine.SearchUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DomainQueryEngine.SearchUI.Controllers
{
    /// <summary>
    /// The search page. GET /ara renders right away with a loading state; the results come from
    /// GET /ara/sonuclar through htmx, so a slow provider never delays the page itself.
    /// </summary>
    [Route("ara")]
    public class SearchController : Controller
    {
        private readonly DomainSearchService _search;

        public SearchController(DomainSearchService search)
        {
            _search = search;
        }

        /// <param name="q">Domain or pasted URL.</param>
        /// <param name="name">Name part, used by forms that add the extension separately (with <paramref name="extension"/>).</param>
        /// <param name="extension">Extension without the dot, e.g. "com.tr".</param>
        [HttpGet("")]
        public IActionResult Index(string? q, string? name, string? extension, bool onlyAvailable = false, int? period = null, ResultSort sort = ResultSort.Price)
        {
            var input = !string.IsNullOrWhiteSpace(q) ? q : Combine(name, extension);
            var filters = Filters(onlyAvailable, period, sort);

            if (string.IsNullOrWhiteSpace(input))
                return RedirectToAction("Index", "Home");

            if (!DomainSearchService.TryNormalize(input, out var domain))
                return View(new SearchPageViewModel { Input = input, ErrorKey = "Search.InvalidDomain", Filters = filters });

            return View(new SearchPageViewModel { Input = input, Domain = domain, Filters = filters });
        }

        /// <param name="reprice">
        /// True when the page re-shows the search (filter or currency change); not counted as a new search.
        /// </param>
        [HttpGet("sonuclar")]
        public async Task<IActionResult> Results(string? q, bool onlyAvailable = false, int? period = null, ResultSort sort = ResultSort.Price, bool reprice = false, CancellationToken ct = default)
        {
            if (!DomainSearchService.TryNormalize(q, out var domain))
                return PartialView("Components/_EmptyState", "Search.InvalidDomain");

            var filters = Filters(onlyAvailable, period, sort);
            var comparison = await _search.SearchAsync(domain, record: !reprice, currency: null, period: filters.Period, ct: ct);

            // Keep the address bar in step with the filters so the page can be shared and reloaded.
            if (Request.Headers.ContainsKey("HX-Request"))
                Response.Headers["HX-Replace-Url"] = Url.Action("Index", new { q = domain, onlyAvailable = filters.OnlyAvailable ? "true" : null, period = filters.Period, sort = filters.Sort == ResultSort.Price ? null : filters.Sort.ToString().ToLowerInvariant() });

            return PartialView("Components/_SearchResults", ResultsViewModel.From(comparison, filters));
        }

        private static SearchFilters Filters(bool onlyAvailable, int? period, ResultSort sort)
            => new(onlyAvailable, period is int p && SearchFilters.Periods.Contains(p) ? p : null, sort);

        private static string? Combine(string? name, string? extension)
            => string.IsNullOrWhiteSpace(name) ? null
             : string.IsNullOrWhiteSpace(extension) ? name.Trim()
             : $"{name.Trim().TrimEnd('.')}.{extension.Trim().TrimStart('.')}";
    }
}
