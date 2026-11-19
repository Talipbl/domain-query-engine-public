using System.Globalization;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;

namespace DomainQueryEngine.SearchUI.Helpers
{
    /// <summary>
    /// Formats prices and dates for the request's language: Turkish puts the symbol after the amount
    /// ("1.540,00 ₺"), English before it ("₺1,540.00").
    /// </summary>
    public static class PriceFormatter
    {
        public static string Format(decimal amount, string currency)
        {
            var culture = CultureInfo.CurrentCulture;
            var number = amount.ToString("N2", culture);
            var symbol = PriceHelper.GetCurrencySymbol(currency);
            return culture.TwoLetterISOLanguageName == "tr" ? $"{number} {symbol}" : $"{symbol}{number}";
        }

        public static string Format(BasePrice price) => Format(price.Amount, price.Currency);

        /// <summary>A plain amount without symbol, e.g. for chart labels.</summary>
        public static string Number(decimal amount) => amount.ToString("N2", CultureInfo.CurrentCulture);

        /// <summary>Short date: 19.11.2026 in Turkish, 11/19/2026 in English.</summary>
        public static string FormatDate(DateTime date) => date.ToString("d", CultureInfo.CurrentCulture);
    }
}
