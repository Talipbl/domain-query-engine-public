using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DomainQueryEngine.SearchUI.TagHelpers
{
    public enum ButtonVariant
    {
        Primary,
        Secondary,
        Link
    }

    public enum ButtonSize
    {
        Sm,
        Md,
        Lg
    }

    /// <summary>
    /// <c>&lt;btn variant="primary" size="lg" href="/x"&gt;</c> renders a link styled as a button when
    /// <c>href</c> is set, otherwise a <c>&lt;button&gt;</c> (type="button" unless given).
    /// <c>external</c> opens the link in a new tab and adds "↗".
    /// </summary>
    [HtmlTargetElement("btn")]
    public class ButtonTagHelper : TagHelper
    {
        public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;
        public ButtonSize Size { get; set; } = ButtonSize.Sm;
        public string? Href { get; set; }
        public bool External { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var isLink = !string.IsNullOrEmpty(Href);
            output.TagName = isLink ? "a" : "button";
            output.TagMode = TagMode.StartTagAndEndTag;

            var classes = $"btn btn--{Variant.ToString().ToLowerInvariant()}";
            if (Size != ButtonSize.Sm)
                classes += $" btn--{Size.ToString().ToLowerInvariant()}";

            var existing = output.Attributes["class"]?.Value?.ToString();
            output.Attributes.SetAttribute("class", string.IsNullOrEmpty(existing) ? classes : $"{classes} {existing}");

            if (isLink)
            {
                output.Attributes.SetAttribute("href", Href);
                if (External)
                {
                    output.Attributes.SetAttribute("target", "_blank");
                    output.Attributes.SetAttribute("rel", "noopener");
                }
            }
            else if (!output.Attributes.ContainsName("type"))
            {
                output.Attributes.SetAttribute("type", "button");
            }

            if (External)
            {
                var content = await output.GetChildContentAsync();
                output.Content.SetHtmlContent(content);
                output.Content.AppendHtml(" <span aria-hidden=\"true\">↗</span><span class=\"sr-only\">(yeni sekmede açılır)</span>");
            }
        }
    }
}
