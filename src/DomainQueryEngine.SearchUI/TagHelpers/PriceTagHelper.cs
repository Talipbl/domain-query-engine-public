using System.Text.Encodings.Web;
using DomainQueryEngine.Core;
using DomainQueryEngine.SearchUI.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace DomainQueryEngine.SearchUI.TagHelpers
{
    /// <summary>
    /// <c>&lt;price-tag price="@display" original="@original" regular="@regular" period="1"&gt;</c>
    /// shows the price in the display currency. When <c>original</c> is in another currency the amount
    /// is marked "≈" and the provider's own price is noted below; <c>regular</c> shows a struck-through
    /// regular price next to a campaign price; <c>warning</c> adds a highlighted note.
    /// </summary>
    [HtmlTargetElement("price-tag")]
    public class PriceTagHelper : TagHelper
    {
        private readonly IStringLocalizer<SharedResource> _text;

        public PriceTagHelper(IStringLocalizer<SharedResource> text)
        {
            _text = text;
        }

        public BasePrice? Price { get; set; }

        /// <summary>The provider's own price, when <see cref="Price"/> was converted from it.</summary>
        public BasePrice? Original { get; set; }

        /// <summary>Regular price when <see cref="Price"/> is a campaign price.</summary>
        public BasePrice? Regular { get; set; }

        /// <summary>Registration period in years; shown as "/ N yıl" when set.</summary>
        public int? Period { get; set; }

        public string? Warning { get; set; }

        /// <summary>Highlights the amount as the best offer.</summary>
        public bool Best { get; set; }

        /// <summary>Left-aligns the block (default is right-aligned, for result rows).</summary>
        public bool AlignStart { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            if (Price == null)
            {
                output.SuppressOutput();
                return;
            }

            var classes = "price" + (Best ? " price--best" : "") + (AlignStart ? " price--start" : "");
            output.Attributes.SetAttribute("class", classes);

            var html = HtmlEncoder.Default;
            var converted = Original != null && !string.Equals(Original.Currency, Price.Currency, StringComparison.OrdinalIgnoreCase);

            var approx = converted ? $"<span class=\"price__approx\" title=\"{html.Encode(_text["Price.Converted"])}\">≈</span> " : "";
            var period = Period is > 0 ? $" <span class=\"price__note\">{html.Encode(_text["Price.PerYears", Period.Value])}</span>" : "";
            output.Content.AppendHtml($"<span class=\"price__amount\">{approx}{html.Encode(PriceFormatter.Format(Price))}{period}</span>");

            var notes = new List<string>();
            if (converted)
                notes.Add(html.Encode(_text["Price.ProviderPrice", PriceFormatter.Format(Original!)]));
            if (Regular != null && Regular.Amount > Price.Amount)
                notes.Add($"{html.Encode(_text["Price.Regular"])} <s>{html.Encode(PriceFormatter.Format(Regular))}</s>");

            if (notes.Count > 0)
                output.Content.AppendHtml($"<span class=\"price__note\">{string.Join(" · ", notes)}</span>");
            if (!string.IsNullOrEmpty(Warning))
                output.Content.AppendHtml($"<span class=\"price__note price__note--warn\">{html.Encode(Warning)}</span>");
        }
    }
}
