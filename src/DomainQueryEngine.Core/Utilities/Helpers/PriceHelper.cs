using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DomainQueryEngine.Core.Utilities
{
    public static class PriceHelper
    {
        private static readonly Dictionary<string, string> CurrencySymbols = new(StringComparer.OrdinalIgnoreCase)
        {
            { "TRY", "₺" },
            { "USD", "$" },
            { "EUR", "€" },
            { "GBP", "£" }
        };

        private static readonly Dictionary<string, string> CurrencyPatterns = new(StringComparer.OrdinalIgnoreCase)
        {
            { "₺|TL|TRY", "TRY" },
            { "\\$|USD", "USD" },
            { "€|EUR", "EUR" },
            { "£|GBP", "GBP" }
        };

        public static (decimal Amount, string Currency) Parse(string priceString)
        {
            if (string.IsNullOrWhiteSpace(priceString))
                return (0, "UNKNOWN");

            var input = priceString.Trim();

            string currency = "UNKNOWN";

            // Currency yakalama ve input'tan çıkarma
            foreach (var kvp in CurrencyPatterns)
            {
                if (Regex.IsMatch(input, kvp.Key, RegexOptions.IgnoreCase))
                {
                    currency = kvp.Value;
                    input = Regex.Replace(input, kvp.Key, "", RegexOptions.IgnoreCase);
                    break;
                }
            }

            // Boşluk, unicode boşluk vs temizle
            input = input.Replace(" ", "").Trim();

            // Sadece rakam, nokta ve virgül kalsın
            input = Regex.Replace(input, @"[^0-9.,]", "");

            if (string.IsNullOrEmpty(input))
                return (0, currency);

            // Hangi karakter binlik, hangisi ondalık?
            // Kural:
            // 1) Hem ',' hem '.' varsa:
            //    - Sağdan bak, en sondaki karakter ondalık ayırıcıdır
            // 2) Sadece virgül varsa → virgül ondalık olabilir
            // 3) Sadece nokta varsa → nokta ondalık olabilir

            char decimalSeparator = DetectDecimalSeparator(input);

            // Binlik ayırıcıları sil
            if (decimalSeparator == ',')
                input = input.Replace(".", "");
            else if (decimalSeparator == '.')
                input = input.Replace(",", "");

            // Şimdi ondalık ayırıcıyı invariant format için noktaya çevir
            input = input.Replace(decimalSeparator, '.');

            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                return (amount, currency);

            return (0, currency);
        }

        public static string GetCurrencySymbol(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                return "";

            return CurrencySymbols.TryGetValue(currencyCode.ToUpperInvariant(), out var symbol) ? symbol : currencyCode;
        }

        private static char DetectDecimalSeparator(string input)
        {
            bool hasDot = input.Contains('.');
            bool hasComma = input.Contains(',');

            if (hasDot && hasComma)
            {
                // İkisi de varsa → sondan bak, hangisi en son ise o ondalık ayrıcıdır.
                int lastDot = input.LastIndexOf('.');
                int lastComma = input.LastIndexOf(',');

                return lastComma > lastDot ? ',' : '.';
            }

            // Tek bir ayırıcı varsa, o ondalıktır
            if (hasComma) return ',';
            if (hasDot) return '.';

            // Hiç yoksa → ondalık yok, fark etmez, nokta kabul et
            return '.';
        }
    }
}
