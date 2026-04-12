using System.Text.RegularExpressions;
using System.Web;

namespace DomainQueryEngine.Core.Utilities
{
    public static class SecurityHelper
    {
        public static string SanitizeSqlInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string pattern = @"('|--|;|/\*|\*/|xp_)";
            return Regex.Replace(input, pattern, "", RegexOptions.IgnoreCase);
        }

        public static string SanitizeXssInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return HttpUtility.HtmlEncode(input);
        }
    }
}
