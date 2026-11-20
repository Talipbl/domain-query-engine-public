using System.Text;
using System.Xml;
using DomainQueryEngine.Core.Abstractions;
using DomainQueryEngine.SearchUI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DomainQueryEngine.SearchUI.Controllers
{
    public class SeoController : Controller
    {
        private readonly SiteUrl _siteUrl;
        private readonly ITldPriceCatalog _tlds;

        public SeoController(SiteUrl siteUrl, ITldPriceCatalog tlds)
        {
            _siteUrl = siteUrl;
            _tlds = tlds;
        }

        /// <summary>Search results, API and preference endpoints are not for indexing.</summary>
        [HttpGet("/robots.txt")]
        public IActionResult Robots()
        {
            var text = new StringBuilder()
                .AppendLine("User-agent: *")
                .AppendLine("Disallow: /ara")
                .AppendLine("Disallow: /api/")
                .AppendLine("Disallow: /tercihler/")
                .AppendLine("Disallow: /_bilesenler")
                .AppendLine($"Sitemap: {_siteUrl.Absolute(Request, "/sitemap.xml")}")
                .ToString();
            return Content(text, "text/plain", Encoding.UTF8);
        }

        [HttpGet("/sitemap.xml")]
        public async Task<IActionResult> Sitemap(CancellationToken ct)
        {
            var lastModified = (await _tlds.GetAsOfAsync(ct)).ToString("yyyy-MM-dd");
            var paths = new List<string>
            {
                Url.Action("Index", "Home")!,
                Url.Action("Index", "Tlds")!
            };
            paths.AddRange((await _tlds.GetAllAsync(ct)).Select(t => Url.Action("Detail", "Tlds", new { slug = t.Slug })!));

            var output = new StringBuilder();
            using (var xml = XmlWriter.Create(new StringWriterWithEncoding(output, Encoding.UTF8), new XmlWriterSettings { Indent = true, Async = true }))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                foreach (var path in paths.Distinct())
                {
                    xml.WriteStartElement("url");
                    xml.WriteElementString("loc", _siteUrl.Absolute(Request, path));
                    xml.WriteElementString("lastmod", lastModified);
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();
                xml.WriteEndDocument();
            }

            return Content(output.ToString(), "application/xml", Encoding.UTF8);
        }

        private sealed class StringWriterWithEncoding : StringWriter
        {
            public StringWriterWithEncoding(StringBuilder builder, Encoding encoding) : base(builder) => Encoding = encoding;
            public override Encoding Encoding { get; }
        }
    }
}
