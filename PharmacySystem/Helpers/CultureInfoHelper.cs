using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Helpers
{
    public  static class CultureInfoHelper
    {
        private static CultureInfo _cultureInfo = new CultureInfo("es-EC");
        public static string CultureInfoConverterDecimal(decimal value)
        {
            return value.ToString("0.00",CultureInfo.InvariantCulture);
        }
        // FormatAsEcuadorCurrency uses es-EC grouping ("." for thousands, "," for decimals), so a
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
        public static string FormatAsEcuadorCurrency(decimal value)
        {
            return value.ToString("C", _cultureInfo);
        }

        public static CultureInfo CustomCultureInfo()
        {
            return _cultureInfo;
        }

    }
}
