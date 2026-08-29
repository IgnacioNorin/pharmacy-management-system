using PharmacySystem.Model;

namespace PharmacySystem.Validators
{
    // Validates the recipient's tax id on a Factura according to the store's country preset.
    // The generic scheme is a format check only (DocumentValidator); a national scheme adds a
    // check-digit algorithm and is opt-in per preset, so the core stays country-neutral.
    public static class RecipientDocumentValidator
    {
        public static bool IsValid(string document, string scheme)
        {
            if (scheme == CountryPresets.ChileanRutScheme)
            {
                return ChileanRutValidator.IsValid(document);
            }

            return DocumentValidator.IsValid(document);
        }
    }
}
