using DomainQueryEngine.Application;
using DomainQueryEngine.Application.Lookup;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Entities;
using DomainQueryEngine.Core.Utilities;
using DomainQueryEngine.SearchUI.Extensions;
using DomainQueryEngine.SearchUI.Helpers;
using DomainQueryEngine.SearchUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DomainQueryEngine.SearchUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        ILookupService _lookupService;
        IDomainService _domainService;
        IProviderService _providerService;
        IHttpContextAccessor _httpContextAccessor;
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly PriceComparisonService _priceComparison;

        private string AccessToken { get; set; }
        public UserAgentModel UserAgent { get; private set; }

        public LookupController(ILookupService lookupService, IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IHttpContextAccessor httpContextAccessor, IDomainService domainService, IProviderService providerService, PriceComparisonService priceComparison)
        {
            _lookupService = lookupService;
            _priceComparison = priceComparison;
            _domainService = domainService;
            _providerService = providerService;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _httpContextAccessor = httpContextAccessor;

            AccessToken = _httpContextAccessor.GetOrSetCookie();
            UserAgent = _httpContextAccessor.GetUserAgent();
        }

        /// <param name="domain">Domain or pasted URL.</param>
        /// <param name="currency">Display currency (TRY, USD or EUR). When omitted, the visitor's saved choice (cookie) is used; anything unsupported falls back to TRY.</param>
        /// <param name="reprice">
        /// True when the page only re-shows the last search in another currency. Such requests are not
        /// saved to the history, so query counts per domain stay accurate.
        /// </param>
        [HttpGet("{domain}")]
        public async Task<IActionResult> DomainLookup(string domain, [FromQuery] string? currency = null, [FromQuery] bool reprice = false)
        {
            string validatedDomain;
            try
            {
                validatedDomain = DomainHelper.ValidateAndNormalizeDomain(domain);
            }
            catch (ArgumentException)
            {
                // The page inserts this response as HTML, so answer with a readable message, not a stack trace.
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Content("<div class=\"empty-state\">Geçerli bir alan adı girin (örnek: ornek.com).</div>", "text/html");
            }

            var results = await _lookupService.LookupDomainAsync(validatedDomain);

            if (!reprice)
            {
                Domain domainDto = await SaveLookupRequest(validatedDomain);
                await SaveLookupResult(domainDto, results);
            }

            // History keeps the providers' original prices; conversion only affects what is shown.
            var comparison = await _priceComparison.CompareAsync(results, currency ?? CurrencyPreference.Get(HttpContext));

            var html = await RenderViewToStringAsync("/Views/Lookup/_Results.cshtml", ResultsViewModel.From(comparison));
            return Content(html, "text/html");
        }

        private async Task SaveLookupResult(Domain domainDto, List<SearchResult> results)
        {
            foreach (var r in results)
            {
                ProviderInfo providerInfo = r.ProviderInfo;

                var provider = await _providerService.GetOrCreateProviderAsync(providerInfo);

                var minPriceEntry = r.Domain?.Prices?.OrderBy(p => p.Period).FirstOrDefault();
                var lookupResult = new DomainLookupResult
                {
                    DomainId = domainDto.Id,
                    ProviderId = provider.Id,
                    IsSuccess = r.Domain.IsSuccess,
                    IsAvailable = r.Domain.IsAvailable,
                    ResponseTimeMs = r.ResponseTimeMs,
                    RawResponse = r.Domain.RawResponse,
                    PriceAmount = minPriceEntry?.RegistrationPrice?.Amount,
                    PriceCurrency = minPriceEntry?.RegistrationPrice?.Currency,
                    PricePeriod = minPriceEntry?.Period,
                    CheckedAt = DateTime.UtcNow
                };

                await _lookupService.AddDomainLookupResult(lookupResult);
            }
        }

        private async Task<Domain> SaveLookupRequest(string domain)
        {
            Domain domainDto = new()
            {
                DomainName = domain,
                Tld = DomainHelper.GetTldFromDomainName(domain),
                CreatedAt = DateTime.UtcNow,
                UserIp = NetworkHelper.GetRequestIp(_httpContextAccessor),
                UserAgent = UserAgent.Agent,
                SessionId = AccessToken
            };

            domainDto = await _domainService.AddDomainSearch(domainDto);
            return domainDto;
        }

        private async Task<string> RenderViewToStringAsync(string viewPath, object model)
        {
            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor);

            var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: false);
            var findViewResult = _viewEngine.FindView(actionContext, viewPath, isMainPage: false);

            var viewResult = (getViewResult.Success ? getViewResult : findViewResult);

            if (!viewResult.Success)
            {
                var searched = string.Join(Environment.NewLine, viewResult.SearchedLocations);
                throw new InvalidOperationException($"View '{viewPath}' not found. Searched: {searched}");
            }

            await using var sw = new StringWriter();
            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var tempData = new TempDataDictionary(HttpContext, _tempDataProvider);

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
