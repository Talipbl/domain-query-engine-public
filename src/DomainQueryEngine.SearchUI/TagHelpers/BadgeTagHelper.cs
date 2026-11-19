using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DomainQueryEngine.SearchUI.TagHelpers
{
    public enum BadgeKind
    {
        Available,
        Taken,
        Transfer,
        Campaign,
        Best,
        Mock,

        /// <summary>Something needs attention (e.g. a provider that is failing).</summary>
        Warning,

        /// <summary>Plain, informational (e.g. a disabled provider).</summary>
        Neutral
    }

    /// <summary>
    /// <c>&lt;badge kind="available"&gt;Müsait&lt;/badge&gt;</c> → <c>&lt;span class="badge badge--available"&gt;</c>.
    /// <c>kind="mock"</c> marks sample data; without content it reads "Örnek veri".
    /// </summary>
    [HtmlTargetElement("badge")]
    public class BadgeTagHelper : TagHelper
    {
        public BadgeKind Kind { get; set; } = BadgeKind.Available;

        /// <summary>Rounded "pill" shape, used for larger status labels.</summary>
        public bool Pill { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;

            var classes = $"badge badge--{Kind.ToString().ToLowerInvariant()}{(Pill ? " badge--pill" : "")}";
            var existing = output.Attributes["class"]?.Value?.ToString();
            output.Attributes.SetAttribute("class", string.IsNullOrEmpty(existing) ? classes : $"{classes} {existing}");

            var content = await output.GetChildContentAsync();
            if (content.IsEmptyOrWhiteSpace && Kind == BadgeKind.Mock)
                output.Content.SetContent("Örnek veri");
        }
    }
}
