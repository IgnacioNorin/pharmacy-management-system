using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmacySystem.Model
{
    // A named bundle of country defaults. It is a starting point the store admin can override
    // field by field (in Gestión de tienda), not a lock. Chile is the only concrete preset
    // today; the generic preset makes no national assumptions.
    public class CountryPreset
    {
        // ISO 3166-1 alpha-2, or "" for the generic preset.
        public string Code { get; }
        public string DisplayName { get; }
        // Percentage applied to tax-affected items, e.g. 19.00.
        public decimal DefaultTaxRate { get; }
        // .NET culture name for currency formatting (see CultureInfoHelper.SupportedCurrencies).
        public string CurrencyCulture { get; }
        // How the recipient's tax id on a Factura is validated: CountryPresets.GenericScheme
        // (format only) or CountryPresets.ChileanRutScheme (modulo 11).
        public string RecipientDocumentScheme { get; }

        public CountryPreset(string code, string displayName, decimal defaultTaxRate, string currencyCulture, string recipientDocumentScheme)
        {
            Code = code ?? "";
            DisplayName = displayName;
            DefaultTaxRate = defaultTaxRate;
            CurrencyCulture = currencyCulture;
            RecipientDocumentScheme = recipientDocumentScheme;
        }

        public bool IsGeneric => Code.Length == 0;
    }

    public static class CountryPresets
    {
        public const string GenericScheme = "generic";
        public const string ChileanRutScheme = "chilean_rut";

        public static readonly CountryPreset Generic =
            new CountryPreset("", "Genérico", 0m, "en-US", GenericScheme);

        public static readonly CountryPreset Chile =
            new CountryPreset("CL", "Chile", 19m, "es-CL", ChileanRutScheme);

        public static readonly IReadOnlyList<CountryPreset> All = new[] { Generic, Chile };

        // Null / empty / unknown code -> the generic preset.
        public static CountryPreset ForCode(string code)
        {
            string wanted = (code ?? "").Trim();
            if (wanted.Length == 0)
            {
                return Generic;
            }

            return All.FirstOrDefault(p => !p.IsGeneric &&
                       string.Equals(p.Code, wanted, StringComparison.OrdinalIgnoreCase))
                   ?? Generic;
        }
    }
}
