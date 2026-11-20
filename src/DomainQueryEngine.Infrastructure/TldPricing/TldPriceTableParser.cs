using DomainQueryEngine.Core.TldPricing;
using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DomainQueryEngine.Infrastructure.TldPricing
{
    /// <summary>Parses common provider price tables by their column headings; unknown layouts fail closed.</summary>
    public sealed class TldPriceTableParser
    {
        private static readonly Regex TldToken = new(@"(?:^|\s)\.?([a-z0-9-]+(?:\.[a-z0-9-]+)*)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex Term = new(@"(?:/\s*|per\s+)?(?<years>\d+)\s*(?:yıl|years?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex Money = new(
            @"(?:(?<currency>USD|TRY|EUR|GBP|TL|₺|\$|€|£)\s*(?<amount>\d[\d.,]*))|(?:(?<amount>\d[\d.,]*)\s*(?<currency>USD|TRY|EUR|GBP|TL|₺|\$|€|£))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public IReadOnlyList<TldProviderPrice> Parse(string html, string providerSlug)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var tables = document.DocumentNode.SelectNodes("//table") ?? new HtmlNodeCollection(document.DocumentNode);
            var parsed = tables.Select(table => ParseTable(table, providerSlug)).OrderByDescending(rows => rows?.Count).FirstOrDefault();
            if (parsed == null || parsed.Count == 0)
                throw new FormatException($"No recognizable TLD registration/renewal table was found for provider '{providerSlug}'.");
            return parsed;
        }

        private static List<TldProviderPrice>? ParseTable(HtmlNode table, string providerSlug)
        {
            var rows = table.SelectNodes(".//tr");
            if (rows == null || rows.Count < 2) return null;

            var headerRow = rows.FirstOrDefault(r => r.SelectNodes("./th")?.Count > 0);
            if (headerRow == null) return null;
            var headers = Cells(headerRow).Select(Text).ToList();
            var tldIndex = Array.FindIndex(headers.ToArray(), h => Has(h, "tld", "extension", "uzant", "domain"));
            var registrationIndex = Array.FindIndex(headers.ToArray(), h => Has(h, "tescil", "kayıt", "kayit", "registration", "register") && !Has(h, "transfer", "renew", "yenile"));
            var renewalIndex = Array.FindIndex(headers.ToArray(), h => Has(h, "renew", "yenile", "uzatma"));
            if (tldIndex < 0 || registrationIndex < 0 || renewalIndex < 0 || registrationIndex == renewalIndex)
                return null;

            var result = new List<TldProviderPrice>();
            var passedHeader = false;
            foreach (var row in rows)
            {
                if (ReferenceEquals(row, headerRow)) { passedHeader = true; continue; }
                if (!passedHeader) continue;
                var cells = Cells(row);
                if (cells.Count <= Math.Max(tldIndex, Math.Max(registrationIndex, renewalIndex))) continue;
                var tld = ParseTld(Text(cells[tldIndex]));
                if (tld == null) continue;

                var registration = ParsePrice(cells[registrationIndex], providerSlug, headers[registrationIndex]);
                var renewal = ParsePrice(cells[renewalIndex], providerSlug, headers[renewalIndex]);
                if (registration.Amount == null && renewal.Amount == null) continue;

                result.Add(new TldProviderPrice
                {
                    Tld = tld,
                    Provider = providerSlug,
                    Currency = registration.Currency ?? renewal.Currency ?? DefaultCurrency(providerSlug),
                    Registration = registration.Amount,
                    RegularRegistration = registration.Regular,
                    RegistrationTermYears = registration.TermYears,
                    Renewal = renewal.Amount,
                    RenewalTermYears = renewal.TermYears
                });
            }
            return result;
        }

        private static (decimal? Amount, decimal? Regular, string? Currency, int TermYears) ParsePrice(HtmlNode cell, string providerSlug, string header)
        {
            var currencyDefault = DefaultCurrency(providerSlug);

            var regularValues = new List<(decimal Amount, string Currency)>();
            var struck = cell.SelectNodes(".//del|.//s|.//strike") ?? new HtmlNodeCollection(cell);
            foreach (var node in struck)
                regularValues.AddRange(ParseMoney(Text(node), currencyDefault));

            var activeCell = cell.CloneNode(true);
            var remove = activeCell.SelectNodes(".//del|.//s|.//strike") ?? new HtmlNodeCollection(activeCell);
            foreach (var node in remove.ToList()) node.Remove();
            var activeText = Text(activeCell);
            var termMatch = Term.Match(activeText);
            if (!termMatch.Success) termMatch = Term.Match(header);
            var termYears = termMatch.Success && int.TryParse(termMatch.Groups["years"].Value, out var years) && years > 0 ? years : 1;
            var activeValues = ParseMoney(activeText, currencyDefault);
            var current = Choose(activeValues, currencyDefault);
            if (current == null) return (null, null, null, termYears);

            var regular = regularValues.Where(v => v.Currency == current.Value.Currency && v.Amount > current.Value.Amount)
                .Select(v => (decimal?)v.Amount).OrderBy(v => v).FirstOrDefault();
            return (current.Value.Amount, regular, current.Value.Currency, termYears);
        }

        private static List<(decimal Amount, string Currency)> ParseMoney(string text, string fallbackCurrency)
        {
            var matches = Money.Matches(text);
            var values = new List<(decimal, string)>();
            foreach (Match match in matches)
            {
                var amountText = match.Groups["amount"].Value.Replace(" ", string.Empty);
                if (!TryParseAmount(amountText, out var amount) || amount <= 0) continue;
                var currency = NormalizeCurrency(match.Groups["currency"].Value);
                values.Add((amount, currency ?? fallbackCurrency));
            }
            if (values.Count == 0)
            {
                var priceText = Term.Replace(text, string.Empty);
                foreach (Match match in Regex.Matches(priceText, @"(?<![\w/])\d[\d.,]*(?![\w/])"))
                {
                    if (TryParseAmount(match.Value, out var amount) && amount > 0)
                        values.Add((amount, fallbackCurrency));
                }
            }
            return values;
        }

        private static (decimal Amount, string Currency)? Choose(List<(decimal Amount, string Currency)> values, string fallbackCurrency)
        {
            if (values.Count == 0) return null;
            var currency = values.Any(v => v.Currency == fallbackCurrency)
                ? fallbackCurrency
                : values[0].Currency;
            return values.Where(v => v.Currency == currency).OrderBy(v => v.Amount).First();
        }

        private static string? ParseTld(string value)
        {
            var normalized = HtmlEntity.DeEntitize(value).Trim().TrimStart('.').ToLowerInvariant();
            var match = TldToken.Match(normalized);
            if (!match.Success) return null;
            var candidate = match.Groups[1].Value.TrimEnd('.');
            if (candidate.Length is < 2 or > 255) return null;
            return candidate.Split('.').All(label => label.Length is >= 1 and <= 63 && label.All(ch => char.IsAsciiLetterOrDigit(ch) || ch == '-'))
                ? candidate
                : null;
        }

        private static bool TryParseAmount(string value, out decimal amount)
        {
            var comma = value.LastIndexOf(',');
            var dot = value.LastIndexOf('.');
            if (comma >= 0 && dot >= 0)
            {
                var decimalSeparator = comma > dot ? ',' : '.';
                var thousandsSeparator = decimalSeparator == ',' ? '.' : ',';
                value = value.Replace(thousandsSeparator.ToString(), string.Empty);
                value = value.Replace(decimalSeparator, '.');
            }
            else if (comma >= 0)
                value = comma == value.Length - 4
                    ? value.Replace(",", string.Empty)
                    : value.Replace(',', '.');
            else if (dot >= 0 && dot == value.Length - 4)
                value = value.Replace(".", string.Empty);

            return decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount);
        }

        private static string? NormalizeCurrency(string value) => value.Trim().ToUpperInvariant() switch
        {
            "TL" or "₺" or "TRY" => "TRY",
            "$" or "USD" => "USD",
            "€" or "EUR" => "EUR",
            "£" or "GBP" => "GBP",
            _ => null
        };

        private static string DefaultCurrency(string providerSlug)
            => providerSlug is "alan-adi-kayit" or "atak-domain" or "ihs" or "natro" or "hosting-com-tr" ? "USD" : "TRY";

        private static List<HtmlNode> Cells(HtmlNode row)
            => row.SelectNodes("./th|./td")?.ToList() ?? new List<HtmlNode>();

        private static string Text(HtmlNode node)
            => HtmlEntity.DeEntitize(node.InnerText).Replace('\u00a0', ' ').Replace('\u202f', ' ').Trim();

        private static bool Has(string value, params string[] terms)
            => terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
