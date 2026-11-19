using DomainQueryEngine.Core;
using DomainQueryEngine.SearchUI.Extensions;
using DomainQueryEngine.SearchUI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DomainQueryEngine.SearchUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEnumerable<IDomainLookupStrategy> _strategies;

        public HomeController(IHttpContextAccessor httpContextAccessor, IEnumerable<IDomainLookupStrategy> strategies)
        {
            _strategies = strategies;
            httpContextAccessor.GetOrSetCookie();
        }

        /// <param name="q">Optional domain to search right away (the page updates this when searching, so results can be shared and reloaded).</param>
        public IActionResult Index(string? q)
        {
            var model = new HomeViewModel
            {
                Query = q?.Trim() ?? string.Empty,
                ProviderNames = _strategies.Select(s => s.Provider.Name).OrderBy(n => n, StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true)).ToList()
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
