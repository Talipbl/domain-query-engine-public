using System.Globalization;
using DomainQueryEngine.Core;
using DomainQueryEngine.Core.Utilities;

namespace DomainQueryEngine.SearchUI.Helpers
{
    /// <summary>
    /// Formats prices the same way everywhere: Turkish number format with the symbol after the amount
    /// ("1.540,00 ₺", "2,99 $").
    /// </summary>
    public static class PriceFormatter
    {
        private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

        public static string Format(decimal amount, string currency)
            => $"{amount.ToString("N2", Turkish)} {PriceHelper.GetCurrencySymbol(currency)}";

        public static string Format(BasePrice price) => Format(price.Amount, price.Currency);

        public static string FormatDate(DateTime date) => date.ToString("dd.MM.yyyy", Turkish);
    }
}
