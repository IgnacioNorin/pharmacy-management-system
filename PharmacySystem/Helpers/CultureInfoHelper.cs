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
        public static decimal CultureInfoConverterStringToDecimal(string value)
        {
            if (value.First() == '$')
            {
                value = value.Replace("$", "");
            }
            value = value.Replace(",", ".");
            return Convert.ToDecimal(value);

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
