using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using PharmacySystem.Model;

namespace PharmacySystem.Helpers
{
    public  static class CultureInfoHelper
    {
        // Neutral fallback used only when no store currency is configured. A country preset
        // (CountryPresets) is what normally sets the store's currency_culture.
        private const string DefaultCurrencyCulture = "en-US";

        // Curated on purpose: an admin can only pick a culture .NET actually formats currency
        // with correctly, instead of typing an arbitrary/invalid culture name into a setting.
        public static readonly IReadOnlyList<ComboBoxItem> SupportedCurrencies = new List<ComboBoxItem>
        {
            new ComboBoxItem { Value = "en-US", Text = "Dólar estadounidense (USD)" },
            new ComboBoxItem { Value = "es-CL", Text = "Peso chileno (CLP)" },
            new ComboBoxItem { Value = "es-MX", Text = "Peso mexicano (MXN)" },
            new ComboBoxItem { Value = "es-CO", Text = "Peso colombiano (COP)" },
            new ComboBoxItem { Value = "es-PE", Text = "Sol peruano (PEN)" },
            new ComboBoxItem { Value = "es-AR", Text = "Peso argentino (ARS)" },
        };

        private static CultureInfo _cultureInfo = new CultureInfo(DefaultCurrencyCulture);

        public static string CultureInfoConverterDecimal(decimal value)
        {
            return value.ToString("0.00",CultureInfo.InvariantCulture);
        }

        // FormatAsCurrency uses the active currency culture's own grouping/decimal separators, so a
        // value it produced (always starting with "$") must be parsed back the same way - naively
        // swapping "," for "." breaks as soon as a thousands separator is present (e.g. "$1.234,50"
        // would become "1.234.50", either throwing or being misread as 123450 depending on the
        // machine's culture). Manually typed input (no "$") has no thousands grouping, so a single
        // "," there is just an alternate decimal separator for the "##.##" hint shown to the user.
        public static decimal CultureInfoConverterStringToDecimal(string value)
        {
            value = value.Trim();

            if (value.StartsWith("$", StringComparison.Ordinal))
            {
                return decimal.Parse(value, NumberStyles.Currency, _cultureInfo);
            }

            if (value.Contains(",") && !value.Contains("."))
            {
                value = value.Replace(",", ".");
            }

            return decimal.Parse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        }

        public static string FormatAsCurrency(decimal value)
        {
            return value.ToString("C", _cultureInfo);
        }

        public static CultureInfo CustomCultureInfo()
        {
            return _cultureInfo;
        }

        // Only accepts a culture name from SupportedCurrencies; anything else (null, a typo, no
        // store setting saved yet) falls back to the default so the app never ends up formatting
        // money with an arbitrary, untested culture.
        public static void SetCurrency(string cultureName)
        {
            ComboBoxItem match = SupportedCurrencies.FirstOrDefault(c => string.Equals((string)c.Value, cultureName, StringComparison.OrdinalIgnoreCase));

            _cultureInfo = new CultureInfo(match != null ? (string)match.Value : DefaultCurrencyCulture);
        }

    }
}
