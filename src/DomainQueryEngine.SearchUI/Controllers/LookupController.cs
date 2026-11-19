using DomainQueryEngine.SearchUI.Models;
using DomainQueryEngine.SearchUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DomainQueryEngine.SearchUI.Controllers
{
    /// <summary>
    /// HTML-fragment lookup API (GET /api/lookup/{domain}). The site's own pages use /ara; this endpoint
    /// stays for existing clients.
    /// </summary>
    [Route("api/[controller]")]
    public class LookupController : Controller
    {
        private readonly DomainSearchService _search;

        public LookupController(DomainSearchService search)
        {
            _search = search;
        }

        /// <param name="domain">Domain or pasted URL.</param>
        /// <param name="currency">Display currency (TRY, USD or EUR). When omitted, the visitor's saved choice (cookie) is used; anything unsupported falls back to TRY.</param>
        /// <param name="reprice">
        /// True when the page only re-shows the last search in another currency. Such requests are not
        /// saved to the history, so query counts per domain stay accurate.
        /// </param>
        [HttpGet("{domain}")]
        public async Task<IActionResult> DomainLookup(string domain, [FromQuery] string? currency = null, [FromQuery] bool reprice = false, CancellationToken ct = default)
        {
            if (!DomainSearchService.TryNormalize(domain, out var normalized))
            {
                // The caller inserts this response as HTML, so answer with a readable message, not a stack trace.
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Content("<div class=\"empty-state\">Geçerli bir alan adı girin (örnek: ornek.com).</div>", "text/html");
            }

            var comparison = await _search.SearchAsync(normalized, record: !reprice, currency, ct: ct);
            return PartialView("Components/_SearchResults", ResultsViewModel.From(comparison));
        }
    }
}
