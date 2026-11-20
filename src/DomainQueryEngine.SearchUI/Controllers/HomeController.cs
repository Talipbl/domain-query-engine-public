using System.Diagnostics;
using DomainQueryEngine.Application.TldPricing;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.TldPricing;
using DomainQueryEngine.SearchUI.Helpers;
using DomainQueryEngine.SearchUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DomainQueryEngine.SearchUI.Controllers
{
    public class HomeController : Controller
    {
        private static readonly TimeSpan PopularWindow = TimeSpan.FromHours(24);

        private readonly TldPriceService _prices;
        private readonly IProviderDirectory _providers;
        private readonly ISearchStatistics _statistics;

        public HomeController(TldPriceService prices, IProviderDirectory providers, ISearchStatistics statistics)
        {
            _prices = prices;
            _providers = providers;
            _statistics = statistics;
        }

        /// <param name="q">Old links searched on the home page (/?q=); they now go to /ara.</param>
        public async Task<IActionResult> Index(string? q, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(q))
                return RedirectToAction("Index", "Search", new { q });

            var tlds = await _prices.GetListAsync(CurrencyPreference.Get(HttpContext), ct: ct);
            var turkish = StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true);

            var priceByTld = tlds.Items.ToDictionary(i => i.Entry.Tld, StringComparer.OrdinalIgnoreCase);
            var mostSearched = await _statistics.GetMostSearchedTldsAsync(PopularWindow, 5, ct);

            return View(new HomeViewModel
            {
                PopularTlds = mostSearched.Select(t => priceByTld.GetValueOrDefault(t.Tld)).Where(i => i != null).Cast<TldListItem>().Take(4).ToList(),
                TldList = tlds,
                MostSearchedTlds = mostSearched
                    .Select(t => new MostSearchedTld(t, priceByTld.ContainsKey(t.Tld)))
                    .ToList(),
                Providers = (await _providers.GetAllAsync(ct))
                    .OrderBy(p => p.Name, turkish)
                    .ToList()
            });
        }

        [Route("gizlilik")]
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>Page for error status codes such as 404 (re-executed by UseStatusCodePagesWithReExecute).</summary>
        [Route("hata/{code:int}")]
        public IActionResult StatusCodePage(int code)
        {
            Response.StatusCode = code;
            return View("StatusCode", code);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
