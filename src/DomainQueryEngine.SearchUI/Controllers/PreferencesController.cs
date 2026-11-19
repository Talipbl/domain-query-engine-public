using DomainQueryEngine.SearchUI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DomainQueryEngine.SearchUI.Controllers
{
    /// <summary>
    /// Saves display preferences in cookies. Without JavaScript the header forms post here and are sent
    /// back to <c>returnUrl</c>; site.js posts with fetch ("X-Requested-With: fetch") and gets 204 instead.
    /// </summary>
    [Route("tercihler")]
    public class PreferencesController : Controller
    {
        [HttpPost("para-birimi")]
        [ValidateAntiForgeryToken]
        public IActionResult Currency(string? currency, string? returnUrl)
        {
            CurrencyPreference.Set(HttpContext, currency);
            return Done(returnUrl);
        }

        /// <param name="theme">"light" or "dark"; anything else clears the choice (follow the system).</param>
        [HttpPost("tema")]
        [ValidateAntiForgeryToken]
        public IActionResult Theme(string? theme, string? returnUrl)
        {
            ThemePreference.Set(HttpContext, theme);
            return Done(returnUrl);
        }

        /// <param name="culture">"tr" or "en".</param>
        [HttpPost("dil")]
        [ValidateAntiForgeryToken]
        public IActionResult Language(string? culture, string? returnUrl)
        {
            LanguagePreference.Set(HttpContext, culture);
            return Done(returnUrl);
        }

        private IActionResult Done(string? returnUrl)
        {
            if (Request.Headers.XRequestedWith == "fetch")
                return NoContent();

            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
        }
    }
}
