using Microsoft.AspNetCore.Mvc;

namespace DomainQueryEngine.SearchUI.Controllers
{
    /// <summary>
    /// Component gallery for building and reviewing the UI. Only available in Development.
    /// </summary>
    [Route("_bilesenler")]
    public class ComponentsController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public ComponentsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            return View();
        }
    }
}
