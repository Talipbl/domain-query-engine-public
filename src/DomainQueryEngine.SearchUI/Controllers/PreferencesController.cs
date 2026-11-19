using DomainQueryEngine.SearchUI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DomainQueryEngine.SearchUI.Controllers
{
    [Route("tercihler")]
    public class PreferencesController : Controller
    {
        /// <summary>
        /// Saves the display currency. Without JavaScript the form posts here and is sent back to
        /// <paramref name="returnUrl"/>; site.js posts with fetch and gets 204 instead.
        /// </summary>
        [HttpPost("para-birimi")]
        [ValidateAntiForgeryToken]
        public IActionResult Currency(string? currency, string? returnUrl)
        {
            CurrencyPreference.Set(HttpContext, currency);

            if (Request.Headers.XRequestedWith == "fetch")
                return NoContent();

            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
        }
    }
}
