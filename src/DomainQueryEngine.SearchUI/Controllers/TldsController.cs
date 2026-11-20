using DomainQueryEngine.Application.TldPricing;
using DomainQueryEngine.SearchUI.Helpers;
using DomainQueryEngine.SearchUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DomainQueryEngine.SearchUI.Controllers
{
    [Route("uzantilar")]
    public class TldsController : Controller
    {
        private readonly TldPriceService _prices;

        public TldsController(TldPriceService prices)
        {
            _prices = prices;
        }

        /// <param name="order">registration, renewal or alphabetical.</param>
        /// <param name="search">Filters extensions by text (".com" finds .com, .com.tr, ...).</param>
        [HttpGet("")]
        public async Task<IActionResult> Index(string? order, string? search, CancellationToken ct)
        {
            var sort = TldListPageViewModel.Sorts.FirstOrDefault(s => s.Key == order) ?? TldListPageViewModel.Sorts[2];

            var list = await _prices.GetListAsync(CurrencyPreference.Get(HttpContext), sort.Sort, ct);
            var items = list.Items.AsEnumerable();
            var text = search?.Trim().TrimStart('.').ToLowerInvariant();
            if (!string.IsNullOrEmpty(text))
                items = items.Where(i => i.Entry.Tld.Contains(text, StringComparison.Ordinal));

            return View(new TldListPageViewModel
            {
                List = list,
                Items = items.ToList(),
                Sort = sort,
                Query = search?.Trim() ?? string.Empty,
            });
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Detail(string slug, CancellationToken ct)
        {
            var detail = await _prices.GetDetailAsync(slug, CurrencyPreference.Get(HttpContext), ct);
            if (detail == null)
                return NotFound();

            // Old or alternative spellings (".com.tr", "COM-TR") redirect to the canonical slug.
            if (!string.Equals(slug, detail.Entry.Slug, StringComparison.Ordinal))
                return RedirectToActionPermanent(nameof(Detail), new { slug = detail.Entry.Slug });

            return View(new TldDetailPageViewModel(detail));
        }
    }
}
