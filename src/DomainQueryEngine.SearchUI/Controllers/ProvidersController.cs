using DomainQueryEngine.Application.TldPricing;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.Core.TldPricing;
using DomainQueryEngine.SearchUI.Helpers;
using DomainQueryEngine.SearchUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DomainQueryEngine.SearchUI.Controllers
{
    [Route("saglayicilar")]
    public class ProvidersController : Controller
    {
        private readonly IProviderDirectory _providers;
        private readonly TldPriceService _prices;

        public ProvidersController(IProviderDirectory providers, TldPriceService prices)
        {
            _providers = providers;
            _prices = prices;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var turkish = StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true);
            var providers = (await _providers.GetAllAsync(ct))
                .OrderBy(p => p.Status)          // active first
                .ThenBy(p => p.Name, turkish)
                .ToList();

            return View(new ProviderListPageViewModel { Providers = providers });
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Detail(string slug, CancellationToken ct)
        {
            var prices = await _prices.GetProviderPricesAsync(slug, CurrencyPreference.Get(HttpContext), ct);
            if (prices == null)
                return NotFound();

            if (!string.Equals(slug, prices.Provider.Slug, StringComparison.Ordinal))
                return RedirectToActionPermanent(nameof(Detail), new { slug = prices.Provider.Slug });

            return View(new ProviderDetailPageViewModel { Prices = prices });
        }
    }
}
