using System;
using System.Globalization;

namespace PharmacySystem.Helpers
{
    // The system operates in Chilean pesos (CLP) only - there is no configurable currency.
    // CLP has no minor unit, so every amount is a whole number of pesos: FormatAsCurrency rounds
    // and prints "$2.000.000" (es-CL: "$" symbol, "." thousands, no decimals), and RoundMoney is
    // applied wherever the user types or the app computes an amount.
    public static class CultureInfoHelper
    {
        private static readonly CultureInfo Clp = CultureInfo.GetCultureInfo("es-CL");

        // Whole pesos, rounded half-away-from-zero.
        public static decimal RoundMoney(decimal value) => Math.Round(value, 0, MidpointRounding.AwayFromZero);

        public static string CultureInfoConverterDecimal(decimal value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        // Parses a money value the user typed or that FormatAsCurrency produced, always rounding
        // to whole pesos. Accepts "$2.000.000", "2.000.000", "2000000". A "," is a decimal point.
        // A "." is the es-CL thousands separator when every group after the first has 3 digits
        // (e.g. "2.000.000"); otherwise the last "." is treated as a decimal point ("10.00" = 10).
        public static decimal CultureInfoConverterStringToDecimal(string value)
        {
            value = (value ?? string.Empty).Trim();

            string currencySymbol = Clp.NumberFormat.CurrencySymbol;
            if (!string.IsNullOrEmpty(currencySymbol) && value.IndexOf(currencySymbol, StringComparison.Ordinal) >= 0)
            {
                return RoundMoney(decimal.Parse(value, NumberStyles.Currency, Clp));
            }

            bool negative = value.StartsWith("-");
            if (negative) value = value.Substring(1).Trim();

            string result;
            if (value.IndexOf(',') >= 0)
            {
                // A comma is unambiguously the decimal separator here; dots are grouping.
                result = value.Replace(".", string.Empty).Replace(',', '.');
            }
            else if (value.IndexOf('.') >= 0)
            {
                string[] groups = value.Split('.');
                bool allThousands = groups.Length > 1;
                for (int i = 1; i < groups.Length && allThousands; i++)
                {
                    if (groups[i].Length != 3) allThousands = false;
                }

                if (allThousands)
                {
                    result = string.Concat(groups); // "2.000.000" -> "2000000"
                }
                else
                {
                    // Last dot is the decimal point: "10.00" -> "10.00", "1.234.5" -> "1234.5".
                    string whole = string.Join(string.Empty, groups, 0, groups.Length - 1);
                    result = whole + "." + groups[groups.Length - 1];
                }
            }
            else
            {
                result = value;
            }

            decimal parsed = decimal.Parse(result, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            return RoundMoney(negative ? -parsed : parsed);
        }

        public static string FormatAsCurrency(decimal value)
        {
            return RoundMoney(value).ToString("C0", Clp);
        }

        // The CLP culture, for a grid column's DefaultCellStyle.FormatProvider.
        public static CultureInfo CustomCultureInfo() => Clp;
    }
}
