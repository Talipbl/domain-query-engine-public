using DomainQueryEngine.Application.Catalog;
using DomainQueryEngine.Core.Catalog;
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

        /// <param name="groupKey">tumu, populer, tr, genel, yeni or ulke.</param>
        /// <param name="order">populerlik, kayit, yenileme or alfabe.</param>
        /// <param name="search">Filters extensions by text (".com" finds .com, .com.tr, ...).</param>
        [HttpGet("")]
        public async Task<IActionResult> Index(string? groupKey, string? order, string? search, CancellationToken ct)
        {
            var group = TldListPageViewModel.Groups.FirstOrDefault(g => g.Key == groupKey) ?? TldListPageViewModel.Groups[0];
            var sort = TldListPageViewModel.Sorts.FirstOrDefault(s => s.Key == order) ?? TldListPageViewModel.Sorts[0];

            var list = await _prices.GetListAsync(CurrencyPreference.Get(HttpContext), (TldCategory?)group.Category, sort.Sort, ct);
            var all = group.Category == null && !group.PopularOnly
                ? list
                : await _prices.GetListAsync(CurrencyPreference.Get(HttpContext), ct: ct);

            var items = list.Items.AsEnumerable();
            if (group.PopularOnly)
                items = items.Where(i => i.Entry.IsPopular);

            var text = search?.Trim().TrimStart('.').ToLowerInvariant();
            if (!string.IsNullOrEmpty(text))
                items = items.Where(i => i.Entry.Tld.Contains(text, StringComparison.Ordinal));

            return View(new TldListPageViewModel
            {
                List = list,
                Items = items.ToList(),
                Group = group,
                Sort = sort,
                Query = search?.Trim() ?? string.Empty,
                GroupCounts = TldListPageViewModel.Groups.ToDictionary(
                    g => g.Key,
                    g => all.Items.Count(i => (g.Category == null || i.Entry.Category == g.Category) && (!g.PopularOnly || i.Entry.IsPopular)))
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
