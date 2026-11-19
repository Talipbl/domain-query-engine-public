using System.Diagnostics;
using DomainQueryEngine.Application.Catalog;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.Catalog;
using DomainQueryEngine.SearchUI.Helpers;
using DomainQueryEngine.SearchUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DomainQueryEngine.SearchUI.Controllers
{
    public class HomeController : Controller
    {
        private static readonly TimeSpan PopularWindow = TimeSpan.FromHours(24);

        private readonly TldPriceService _prices;
        private readonly IProviderCatalog _providers;
        private readonly ISearchStatistics _statistics;

        public HomeController(TldPriceService prices, IProviderCatalog providers, ISearchStatistics statistics)
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

            return View(new HomeViewModel
            {
                PopularTlds = tlds.Items.Where(i => i.Entry.IsPopular).Take(4).ToList(),
                TldList = tlds,
                MostSearched = (await _statistics.GetMostSearchedAsync(PopularWindow, 5, ct)).ToList(),
                MostSearchedIsSample = _statistics.IsSample,
                ActiveProviders = (await _providers.GetAllAsync(ct))
                    .Where(p => p.Status == ProviderStatus.Active)
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
