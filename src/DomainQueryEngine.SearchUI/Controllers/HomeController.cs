using DomainQueryEngine.SearchUI.Extensions;
using DomainQueryEngine.SearchUI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DomainQueryEngine.SearchUI.Controllers
{
    public class HomeController : Controller
    {
        IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpContextAccessor.GetOrSetCookie();
        }

        public IActionResult Index()
        {
            return View();
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
